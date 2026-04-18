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
            1 => "Coroa do Vazio Primordial",
            2 => "Orbe da Singularidade Arcana",
            3 => "Armadura do Colosso Eterno",
            4 => "Anel da Última Realidade",
            5 => "Cajado do Deus Esquecido",
            6 => "Amuleto do Julgamento Absoluto",
            7 => "Espada do Apocalipse Silente",
            8 => "Escudo da Ruína Imortal",
            9 => "Relíquia da Criação Quebrada",
            _ => "Desconhecido"
        };


        public void gerarArquivo()
        {
            Stopwatch sws = new Stopwatch();
            sws.Start();
            using (StreamWriter sw = new StreamWriter(path))
            {
                for (long i = 0; i < 74902939; i++)
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
                            // int lines = 0;
                            long capacity = accessor.Capacity;
                            while (posicaoArquivo < capacity)
                            {

                                long restante = capacity - posicaoArquivo;
                                int tamanhoChunk = (int)Math.Min(restante, 1024);
                                //byte[] buffer = new byte[10000];
                                ReadOnlySpan<byte> fileSpan = new ReadOnlySpan<byte>(pointerRead + posicaoArquivo, tamanhoChunk);
                                //accessor.ReadArray(posicaoArquivo, buffer, 0, buffer.Length);
                                // int finalLine = fileSpan.Slice(posicaoArquivo).IndexOf((byte)'\n');
                                ReadOnlySpan<byte> quebraLinha = "\r\n"u8; // O sufixo u8 converte a string direto para bytes (UTF-8)
                                int finalLine = fileSpan.IndexOf(quebraLinha);

                                //ReadOnlySpan<byte> Teste = fileSpan.Slice(0, posicaoArquivo);
                                //string contents = Encoding.UTF8.GetString(Teste);
                                if (finalLine == -1)
                                {

                                    CalcularValorTotal(ref dados);
                                    return;
                                }
                                ReadOnlySpan<byte> line = fileSpan.Slice(0, finalLine);
                                //string content = Encoding.UTF8.GetString(line);
                                InterpretaLinha(line, ref dados);

                                // Console.Write($"linha inicial{posicaoArquivo} linha final: {finalLine} ");
                                posicaoArquivo += finalLine + 2;
                                //lines++;
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
                //if (d.Count == null)
                //{
                //    continue;
                //}
                ReadOnlySpan<char> nomeItem = ObterNomeItem(i);
                var lucro = d.TotalSell - d.TotalBuy;
                Console.WriteLine($"Key Item: {i} | Nome Item:{nomeItem} |Valor Total Comprado: {d.TotalBuy:N2} | Valor Total Vendido: {d.TotalSell:N2} | Total Profit: {lucro:N2} | Numero de Vendas: {d.Count:N2}");
            }
        }

        public void InterpretaLinha(ReadOnlySpan<byte> line ,  ref Span<DadosContabilizados> dados )
        {

            //ReadOnlySpan<byte> Testes = line.Slice(0, line.Length);
            //string content = Encoding.UTF8.GetString(Testes);
            int key = 0;
            //int posData = 0;    
            int pipeData = line.IndexOf((byte)'|');

            line = line.Slice(pipeData + 1);
            int pipeKey = line.IndexOf((byte)'|');

             if(Utf8Parser.TryParse(line.Slice(0,pipeKey), out int value, out int bytesConsumed)){
                key = value;
                dados[value].key = value;
                //Console.WriteLine($"Valor da chave: {value}");
                //Console.WriteLine($"Bytes consumidos: {bytesConsumed}");
            }

            line = line.Slice(pipeKey + 1);
            int pipePriceBuy = line.IndexOf((byte)'|');

            if (Utf8Parser.TryParse(line.Slice(0, pipePriceBuy), out double buy, out int bytesConsumedBuy))
            {
                dados[key].TotalBuy += buy;
                //Console.WriteLine($"Valor da chave: {buy}");
                //Console.WriteLine($"Bytes consumidos: {bytesConsumed}");
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


                //ReadOnlySpan<byte> Teste = line.Slice(0, line.Length);
            //string contents = Encoding.UTF8.GetString(Teste);
            if (Utf8Parser.TryParse(line.Slice(0, pipePriceSell), out double sell, out int bytesConsumedSell))
            {
                dados[key].TotalSell += sell;
                dados[key].Count++;
                //Console.WriteLine($"Valor da chave: {value}");
                //Console.WriteLine($"Bytes consumidos: {bytesConsumed}");
            }
        }
    }
}
