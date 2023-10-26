using I = itk.simple;
using V = VMS.TPS.Common.Model.API;

namespace MAAS_TXIHelper.Models
{
    public class ImageModel
    {
        public V.Image VImage { get; set; }
        public string DisplayName { get; set; }
        public int[,] VoxelPlane { get; set; }// = new int[VImage.XSize, VImage.YSize];
        public int[,,] VoxelVolume { get; set; } //= new int[VImage.XSize, VImage.YSize, VImage.ZSize];
        public double[,,] HuValues { get; set; }//= new double[VImage.XSize, VImage.YSize, VImage.ZSize];

        public int ZSize { get; set; }
        public ImageModel(V.Image vImage)
        {
            VImage = vImage;
            DisplayName = $"{VImage.Series.Id} ({VImage.Id})";
            ZSize = VImage.ZSize;

            VoxelPlane = new int[VImage.XSize, VImage.YSize];
            VoxelVolume = new int[VImage.XSize, VImage.YSize, VImage.ZSize];
            HuValues = new double[VImage.XSize, VImage.YSize, VImage.ZSize];
        }

        public I.Image BuildITKImage(double ZSpacingMM)
        {
            I.PixelIDValueEnum pixelType = I.PixelIDValueEnum.sitkFloat32;
            I.VectorUInt32 image3DSize = new I.VectorUInt32(
                new uint[] { (uint)VImage.XSize, (uint)VImage.YSize, (uint)VImage.ZSize });
            I.Image itkImage = new I.Image(image3DSize, pixelType);
            I.VectorDouble spacing3D = new I.VectorDouble(new double[] { VImage.XRes, VImage.YRes, ZSpacingMM });
            itkImage.SetSpacing(spacing3D);
            I.VectorDouble origin = new I.VectorDouble(new double[] { VImage.Origin.x, VImage.Origin.y, VImage.Origin.z });
            itkImage.SetOrigin(origin);
            return itkImage;
        }
    }
}
