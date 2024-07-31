
using System.Diagnostics;
using ComputeSharp;
using GpuCompute.ComputeShader;
using GpuCompute.Model;

// ThreadGroupX批次執行最高上限
const int MaxmaCount = 4194240;

const float PI = 3.14159265358979323846F;

try
{
    var st = new Stopwatch();

    Console.WriteLine($"MaxmaCount: {MaxmaCount}");
    Console.WriteLine("-------------------------------------------------------");
    Console.WriteLine("");

    // CPU
    {
        var coreCount = 0;

        foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
        {
            coreCount += int.Parse(item["NumberOfCores"].ToString());
        }

        Console.WriteLine($"Number Of Cores: {coreCount}");

        foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
        {
            Console.WriteLine($"Number Of Logical Processors: {item["NumberOfLogicalProcessors"]}");
        }

        Console.WriteLine($"The number of processors on this computer is {Environment.ProcessorCount}.");
        Console.WriteLine(">>");

        st.Restart();

        var records = Enumerable.Range(1, MaxmaCount).Select(index => new Circle()
        {
            Rradius = 1.1F * index
        }).ToList();

        var task = Parallel.ForEach(records, record =>
        {
            record.Area = (float)Math.Pow(record.Rradius, 2) * PI;
        });

        while (!task.IsCompleted)
        {
            SpinWait.SpinUntil(() => false, 500);
        }

        st.Stop();

        Console.WriteLine($"CPU Spend:{st.ElapsedMilliseconds} ms");
        Console.WriteLine("-------------------------------------------------------");
        Console.WriteLine("");
    }

    // GPU
    {
        var gd = GraphicsDevice.GetDefault();

        if (gd != null)
        {
            Console.WriteLine($"Device: {gd.Name}");
            Console.WriteLine($"GPU CUDA: {gd.ComputeUnits}");
            Console.WriteLine(">>");

            st.Restart();

            var records = Enumerable.Range(1, MaxmaCount).Select(index => new Circle()
            {
                Rradius = 1.1F * index
            }).ToArray();

            var buffer = gd.AllocateReadWriteBuffer(records);

            gd.For(buffer.Length, new CircleComputeShader(buffer));
            buffer.CopyTo(records);

            st.Stop();

            Console.WriteLine($"GPU Spend: {st.ElapsedMilliseconds} ms");
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

Console.Write("Finished!");
Console.Read();
