using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;

using System.Diagnostics;

namespace POCFederacionCafeteros.Extensions
{
  public static class Extensions
  {
    public static string ToDms(this double value, string axis)
    {
      var sSing = string.Empty;
      var sign = Math.Sign(value);
      value = Math.Abs(value);

      if(!string.IsNullOrEmpty(axis))
      {
        if(sign > 0)
        {
          sSing = axis.ToUpper() == "X" ? "E" : "N";
        }
        else
        {
          sSing = axis.ToUpper() == "X" ? "W" : "S";
        }
        sSing = sign > 0
          ? sign > 0 ? axis.ToUpper() == "X" ? "E" : "N" : axis.ToUpper() == "X" ? "E" : "S"
          : axis.ToUpper() == "X" ? "W" : sign > 0 ? "N" : "S";
        sign = 1;
      }

      var degree = Math.Floor(value);
      var minutes = Math.Floor((value - degree) * 60);
      var seconds = ((value - degree) * 60 - minutes) * 60;

      return string.Format("{0:0}°{1:00}'{2:00.000}\" {3}", sign * degree, minutes, seconds, sSing);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapPoint"></param>
    /// <returns></returns>
    public static string ToDms(this MapPoint mapPoint)
    {
      return $"Y:{mapPoint.Y.ToDms("Y")}, X:{mapPoint.X.ToDms("X")}";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vp"></param>
    /// <param name="otro"></param>
    /// <returns></returns>
    public static bool AreEquals(this Viewpoint vp, Viewpoint otro)
    {

      var json = vp.ToJson();
      var jsonOtro = otro != null ? otro.ToJson() : string.Empty;
      Debug.WriteLine($"Viewpoint 1:{json}");
      Debug.WriteLine($"Viewpoint 2:{jsonOtro}");

      return json == jsonOtro;
    }
  }
}
