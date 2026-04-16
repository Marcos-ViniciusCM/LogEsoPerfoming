using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogEsoPerfomace
{
    public class GeraArquivo
    {

        string path = @"C:\Users\log\log.txt";
        const double maxValue = 100;
        public void gerarArquivo()
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                for (int i = 0; i < 10; i++)
                {
                    string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    int keyItem = Random.Shared.Next(0, 11); ;
                    Double priceBuy = Random.Shared.NextDouble() * maxValue;
                    Double priceBuyRounded = Math.Round(priceBuy, 2);
                    Double priceSell = Random.Shared.NextDouble() * maxValue;
                    Double priceSellRounded = Math.Round(priceSell, 2);
                    sw.WriteLine($"{timeStamp}|{keyItem}|{priceBuyRounded}|{priceSellRounded}");
                }
            }
        }

        public void LerArquivo()
        {
            using (var mmf = MemoryMappedFile.CreateFromFile(path, FileMode.Open))
            {
                long posicaoAtual = 0;
                using (var accessor = mmf.CreateViewAccessor())
                {
                    while (posicaoAtual < accessor.Capacity)
                    {


                        long capacity = 28;
                        byte[] buffer = new byte[capacity];

                        accessor.ReadArray(posicaoAtual, buffer, 0, buffer.Length);


                        string content = Encoding.UTF8.GetString(buffer);


                        Console.Write(content);
                        posicaoAtual += capacity;
                    }
                }
            }
        }

        public void InterpretaLinha(string line, ref int qtBytes)
        {
            if (string.IsNullOrWhiteSpace(line)) return;

            string[] parts = line.Split('|');

            // Verificamos se temos as 4 partes e se os números são válidos
            if (parts.Length == 4 &&
                int.TryParse(parts[1], out int keyItem) &&
                double.TryParse(parts[2], out double priceBuy) &&
                double.TryParse(parts[3], out double priceSell))
            {
                // Se entrou aqui, a linha está 100% íntegra
                double profit = priceSell - priceBuy;

                // Atualizamos o array usando o keyItem como índice (O(1) performance)
                stats[keyItem].TotalBuy += priceBuy;
                stats[keyItem].TotalSell += priceSell;
                stats[keyItem].TotalProfit += profit;
                stats[keyItem].Count++;
            }
            // Se a linha estiver mal formatada ou cortada, o código simplesmente ignora
            // e não quebra a execução.
        }
    }
}
