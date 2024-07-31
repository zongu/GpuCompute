
namespace GpuCompute.ComputeShader
{
    using ComputeSharp;
    using GpuCompute.Model;

    [ThreadGroupSize(DefaultThreadGroupSizes.X)]
    [GeneratedComputeShaderDescriptor]
    internal readonly partial struct CircleComputeShader(ReadWriteBuffer<Circle> buffer) : IComputeShader
    {
        private const float PI = 3.14159265358979323846F;

        public void Execute()
        {
            buffer[ThreadIds.X].Area = Hlsl.Pow(buffer[ThreadIds.X].Rradius, 2F) * PI;
        }
    }
}
