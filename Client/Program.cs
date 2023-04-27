using System.Net.Sockets;
using System.Text;
using TestTaskCar;
using System.Xml.Serialization;

namespace Client
{
	internal class Program
	{
		async static Task Main(string[] args)
		{
			using var tcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			try
			{	
				await tcpClient.ConnectAsync("127.0.0.1", 8888);
				Console.WriteLine("Установлено соединение с сервером");
				using var stream = new NetworkStream(tcpClient);

				while (true)
				{
					var response = new List<byte>();

					Console.WriteLine("Введите номер записи или -а, чтобы получить все: ");
					var input = Console.ReadLine()!;

					//проверка введенных знечений и отправка строки запроса на сервер
					if (int.TryParse(input, out int num) || input == "-a")
						await stream.WriteAsync(Encoding.ASCII.GetBytes($"GET {(input == "-a" ? "-a" : num)}\n"));
					else
						continue;

					//считывание результата
					while (true)
					{
						var data = stream.ReadByte();
						if (data == -1 || data == '\n') 
							break;
						response.Add((byte)data);
					}

					//разделение отдельных записей и их парсинг
					var end = ^0;
					var cars = new List<Car>();
					for (int i = response.Count - 1; i >= 0; i--)
					{
						if (response[i] == 0x02)
						{
							var car = Car.Parse(response.Take(i..end).ToArray());
							end = ^(response.Count - i - 1);
							cars.Add(car);
						}
					}
					Console.WriteLine($"Получено {cars.Count}\n");

					//вывод записей в консоль
					cars.Reverse();
					cars.ForEach(c => Console.WriteLine(c + "\n"));

					//запись в xml
					var formatter = new XmlSerializer(typeof(List<Car>));
					using var fs = new FileStream("cars.xml", FileMode.Create);
					formatter.Serialize(fs, cars);
					Console.WriteLine("Результат сохранен в cars.xml\n");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}