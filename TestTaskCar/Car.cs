using System.Text;

namespace TestTaskCar
{
	public class Car
	{
		public string? Brand { get; set; }
		public ushort? YearOfRelease { get; set; }
		public float? EngineCapacity { get; set; }
		public ushort? NumberOfDoors { get; set; }

		public override string ToString()
		{
			return $"Марка: {Brand}\nГод выпуска: {YearOfRelease}\nОбъем двигателя: {EngineCapacity}\nЧисло дверей: {NumberOfDoors}";
		}

		public byte[] ToByteArray()
		{
			var bytes = new List<byte>();
			var num = 0;

			if (Brand != null) 
			{
				num++;
				bytes.Add(0x09);
				foreach (var item in Encoding.ASCII.GetBytes(Brand))
					bytes.Add(item);
			}
			if (YearOfRelease != null)
			{
				num++;
				bytes.Add(0x12);
				foreach (var item in BitConverter.GetBytes((ushort)YearOfRelease))
					bytes.Add(item);		
			}
			if (EngineCapacity != null)
			{
				num++;
				bytes.Add(0x13);
				foreach (var item in BitConverter.GetBytes((float)EngineCapacity))
					bytes.Add(item);				
			}
			if (NumberOfDoors != null)
			{
				num++;
				bytes.Add(0x12);
				foreach (var item in BitConverter.GetBytes((ushort)NumberOfDoors))
					bytes.Add(item);
			}

			return bytes
				.Prepend((byte)num)
				.Prepend((byte)0x02)
				.ToArray();
		}

		static public Car Parse(byte[] bytes)
		{
			var car = new Car();
			var num = (int)bytes[1];
			var c = 2;
			var isEngineCapacity = false;
			for (int i = 0; i < num; i++)
			{
				if (c < bytes.Length && bytes[c] == 0x09)
				{
					var end = 0;
					for (int j = c + 1; j < bytes.Length; j++)
					{
						if (bytes[j] == 0x12 
							|| bytes[j] == 0x13)
							break;
						end = j+1;
					}
					car.Brand = Encoding.ASCII.GetString(bytes[(c+1)..end]);
					c = end;
				}

				if (c < bytes.Length && bytes[c] == 0x13)
				{
					car.EngineCapacity = BitConverter.ToSingle(bytes[(c+1)..(c+5)], 0);
					c += 5;
					isEngineCapacity = true;
				}

				if (c < bytes.Length && bytes[c] == 0x12)
				{
					if(car.YearOfRelease == null && !isEngineCapacity)
						car.YearOfRelease = BitConverter.ToUInt16(bytes[(c+1)..(c+3)], 0);
					else
						car.NumberOfDoors = BitConverter.ToUInt16(bytes[(c+1)..(c+3)], 0);
					c += 3;
				}
			}

			return car;
		}
	}
}