using Microsoft.Win32.SafeHandles;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Diagnostics;

namespace LogEsoPerfomace
{
    public class GeraArquivo
    {

        string path = @"C:\Users\log\log.txt";
        const double maxValue = 199.99;



        private static ReadOnlySpan<char> ObterNomeItem(int index) => index switch
        {
            0 => "Lâmina do Eclipse Final",
            1 => "Coroa do Vazio",
            2 => "Orbe da Singularidade",
            3 => "Armadura do Colosso",
            4 => "Anel da Última Realidade",
            5 => "Cajado do Esquecido",
            6 => "Amuleto do Julgamento",
            7 => "Espada do Apocalipse",
            8 => "Escudo da Ruína",
            9 => "Relíquia da Criação",
            _ => "Desconhecido"
        };


        public void gerarArquivo()
        {
            Stopwatch sws = new Stopwatch();
            sws.Start();
            using (StreamWriter sw = new StreamWriter(path))
            {
                for (long i = 0; i < 81002939; i++)
                {
                    string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    int keyItem = Random.Shared.Next(0, 10); ;
                    Double priceBuy = Random.Shared.NextDouble() * maxValue;
                    Double priceBuyRounded = Math.Round(priceBuy, 2);
                    Double priceSell = Random.Shared.NextDouble() * maxValue;
                    Double priceSellRounded = Math.Round(priceSell, 2);
                    sw.WriteLine(FormattableString.Invariant($"{timeStamp}|{keyItem}|{priceBuyRounded:F2}|{priceSellRounded:F2}"));                }
            }
            sws.Stop();
            long tempoEmMs = sws.ElapsedMilliseconds;
            Console.WriteLine($"Tempo decorrido para escrever o arquivo: {tempoEmMs} ms");
        }

        public void LerArquivo()
        {
            Stopwatch sw = new Stopwatch();

            sw.Start();
            unsafe
            {
                Span<DadosContabilizados> dados = stackalloc DadosContabilizados[10];

                using (var mmf = MemoryMappedFile.CreateFromFile(path, FileMode.Open))
                {
                    using (var accessor = mmf.CreateViewAccessor())
                    {


                        long posicaoArquivo = 0;
                        byte* pointerRead = null;
                        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointerRead);
                        try
                        {
    
                            long capacity = accessor.Capacity;
                            while (posicaoArquivo < capacity)
                            {

                                long restante = capacity - posicaoArquivo;
                                int tamanhoChunk = (int)Math.Min(restante, 1024);
                                ReadOnlySpan<byte> fileSpan = new ReadOnlySpan<byte>(pointerRead + posicaoArquivo, tamanhoChunk);
                                ReadOnlySpan<byte> quebraLinha = "\r\n"u8; // O sufixo u8 converte a string direto para bytes (UTF-8)
                                int finalLine = fileSpan.IndexOf(quebraLinha);


                                if (finalLine == -1)
                                {

                                    CalcularValorTotal(ref dados);
                                    return;
                                }
                                ReadOnlySpan<byte> line = fileSpan.Slice(0, finalLine);
                                InterpretaLinha(line, ref dados);

                                posicaoArquivo += finalLine + 2;
                            }

                        }
                        finally
                        {
                            accessor.SafeMemoryMappedViewHandle.ReleasePointer();
                            sw.Stop();
                            long tempoEmMs = sw.ElapsedMilliseconds;
                            Console.WriteLine($"Tempo decorrido para ler o arquivo: {tempoEmMs} ms");
                        }
                    }
                }


            }
            
        }


        public void CalcularValorTotal(ref Span<DadosContabilizados> dados)
        {
            for (int i = 0; i < dados.Length; i++)
            {
                ref var d = ref dados[i];
                ReadOnlySpan<char> nomeItem = ObterNomeItem(i);
                var lucro = d.TotalSell - d.TotalBuy;
                Console.WriteLine($"Key: {i}| Nome:{nomeItem} |Vl.Comprado: {d.TotalBuy:N2} |Vl.Vendido: {d.TotalSell:N2} | Profit: {lucro:N2} |Nr.Vendas: {d.Count:N2}");
            }
        }

        public void InterpretaLinha(ReadOnlySpan<byte> line ,  ref Span<DadosContabilizados> dados )
        {

            int key = 0;   
            int pipeData = line.IndexOf((byte)'|');

            line = line.Slice(pipeData + 1);
            int pipeKey = line.IndexOf((byte)'|');

             if(Utf8Parser.TryParse(line.Slice(0,pipeKey), out int value, out int bytesConsumed)){
                key = value;
                dados[value].key = value;

            }

            line = line.Slice(pipeKey + 1);
            int pipePriceBuy = line.IndexOf((byte)'|');

            if (Utf8Parser.TryParse(line.Slice(0, pipePriceBuy), out double buy, out int bytesConsumedBuy))
            {
                dados[key].TotalBuy += buy;
            }

            line = line.Slice(pipePriceBuy + 1);
            int pipePriceSell = line.IndexOf((byte)'\r');
            if(pipePriceSell == -1)
            {
                pipePriceSell = line.IndexOf((byte)'|');
            }
            if(pipePriceSell == -1)
            {
                pipePriceSell = line.Length;
            }


            if (Utf8Parser.TryParse(line.Slice(0, pipePriceSell), out double sell, out int bytesConsumedSell))
            {
                dados[key].TotalSell += sell;
                dados[key].Count++;
            }
        }
    }
}
