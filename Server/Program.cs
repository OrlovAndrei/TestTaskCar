using System.Net.Sockets;
using System.Net;
using System.Text;
using TestTaskCar;

namespace Server
{
	internal class Program
	{
		static public List<Car> _cars = new()
		{
			new Car()
			{
				Brand = "Nissan",
				YearOfRelease = 2008,
				EngineCapacity = 1.6f,
				NumberOfDoors = 4
			},
			new Car()
			{
				Brand = "Volvo",
				YearOfRelease = 2016,
				EngineCapacity = 1.8f,
				NumberOfDoors = 2
			},
			new Car()
			{
				Brand = "Lada",
				YearOfRelease = 2004,
				EngineCapacity = 1.0f,
				NumberOfDoors = 4
			},
			new Car()
			{
				Brand = "Mersedes",
				YearOfRelease = 2020,
				EngineCapacity = 1.5f,
				NumberOfDoors = 4
			},
		};

		async static Task Main(string[] args)
		{
			using var tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			try
			{
				tcpListener.Bind(new IPEndPoint(IPAddress.Any, 8888));
				tcpListener.Listen();
				Console.WriteLine("Сервер запущен... ");

				while (true)
				{
					//создание соединения с клиентом
					var tcpClient = await tcpListener.AcceptAsync();
					Task.Run(async () => await ProcessClientAsync(tcpClient));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		async static Task ProcessClientAsync(Socket tcpClient)
		{
			try
			{
				var stream = new NetworkStream(tcpClient);
				Console.WriteLine($"Соединение с {tcpClient.RemoteEndPoint} установлено");

				while (true)
				{
					var response = new List<byte>();

					//получение запроса
					while (true)
					{
						var data = stream.ReadByte();
						if (data == -1 || data == '\n')
							break;
						response.Add((byte)data);
					}

					var word = Encoding.ASCII.GetString(response.ToArray());
					Console.WriteLine($"Запрос: {word}");
					var param = word[4..^0];

					//отправка всех записей
					if (param == "-a")
						foreach (var car in _cars)
							await stream.WriteAsync(car.ToByteArray());

					//отправка записи по номеру
					else if (int.Parse(param) < _cars.Count)
						await stream.WriteAsync(_cars[int.Parse(param)].ToByteArray());

					//символ завершения
					await stream.WriteAsync(new byte[] { (byte)'\n' });
					Console.WriteLine("Данные отправлены");
				}
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				Console.WriteLine($"Соединение с {tcpClient.RemoteEndPoint} закрыто");
				tcpClient.Close();
			}

		}
	}
}