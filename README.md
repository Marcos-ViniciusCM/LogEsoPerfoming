# 🚀 High Performance Log Processor (.NET)

Este projeto é um estudo de caso sobre **performance extrema** e **gerenciamento de memória** em C#. O objetivo foi criar um motor de processamento capaz de ler e analisar arquivos massivos (3.5GB+) com **alocação zero no Heap**, garantindo que o Garbage Collector (GC) permaneça ocioso durante toda a operação.

## 📊 Performance Benchmark
Os testes foram realizados com um arquivo gerado sinteticamente com **80.902.939 linhas**.

| Operação | Volume | Tempo |
| :--- | :--- | :--- |
| **Escrita (Geração)** | 3.5 GB | ~112s |
| **Leitura & Análise** | 3.5 GB | **2.5s** |
| **Uso de Heap** | - | **Praticamente 0** |

---

## 🧠 A Lógica de Funcionamento

Diferente de abordagens tradicionais que utilizam streams e alocações de strings, este projeto opera em nível de memória e ponteiros:

1.  **Mapeamento de Arquivo (MemoryMappedFile):** O arquivo não é "lido" da forma convencional. Ele é mapeado para o espaço de endereçamento de **memória virtual** do SO. Isso permite acessar os dados do disco evitando cópias intermediárias para buffers, permitindo que a CPU acesse os dados diretamente.
2.  **Acesso via Ponteiros (Unsafe):** Através do `SafeMemoryMappedViewHandle`, o sistema adquire um ponteiro bruto (`byte*`) para o início do arquivo, eliminando camadas de abstração do .NET.
3.  **Processamento por Chunks com Span:** * O sistema calcula o restante do arquivo e define um chunk de leitura (ex: 1024 bytes).
    * Um `ReadOnlySpan<byte>` é criado a partir do ponteiro atual, permitindo o fatiamento (slicing) dos dados sem cópias.
4.  **Slicing e Identificação de Linhas:** * Utilizamos o `IndexOf` diretamente no Span de bytes para localizar a quebra de linha (`\r\n`). Isso isola a linha para processamento sem converter bytes em strings.
5.  **Avanço de Ponteiro:** A posição global do arquivo avança exatamente o tamanho da linha processada (+2 bytes do `\r\n`), preparando o início do próximo ciclo.

---

## 🛠️ Tecnologias e Conceitos Chave

* **MemoryMappedFiles:** Para I/O de alta velocidade via memória virtual.
* **Unsafe / Pointers:** Para evitar overhead de gerenciamento do runtime em loops críticos.
* **Span<T> & stackalloc:** Para garantir que os dados de contabilização vivam apenas na **Stack**, evitando alocações no Heap.
* **Utf8Parser:** Para converter bytes diretamente em tipos numéricos (`int`, `double`) sem criar objetos intermediários.

---

## 🏗️ Estrutura do Log
O arquivo segue o formato delimitado por pipes (`|`):
`TIMESTAMP (yyyyMMddHHmmss) | ID_ITEM | PRECO_COMPRA | PRECO_VENDA`

---
