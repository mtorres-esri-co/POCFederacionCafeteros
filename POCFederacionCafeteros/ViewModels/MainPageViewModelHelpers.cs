using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;

using Drawing = System.Drawing;

namespace POCFederacionCafeteros.ViewModels
{

  /// <summary>
  /// 
  /// </summary>
  internal static class MainPageViewModelHelpers
  {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rendererType"></param>
    /// <returns></returns>
    internal static Renderer CreateRenderer(GeometryType rendererType)
    {
      // Return a simple renderer to match the geometry type provided
      Symbol sym = null;

      switch(rendererType)
      {
        case GeometryType.Point:
        case GeometryType.Multipoint:
          // Create a marker symbol
          sym = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, Drawing.Color.Red, 18);
          break;

        case GeometryType.Polyline:
          // Create a line symbol
          sym = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Drawing.Color.Green, 3);
          break;

        case GeometryType.Polygon:
          // Create a fill symbol
          var lineSym = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Drawing.Color.Black, 2);
          sym = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Drawing.Color.FromArgb(128, Drawing.Color.Gray), lineSym);
          break;

        default:
          break;
      }

      // Return a new renderer that uses the symbol created above
      return new SimpleRenderer(sym);
    }
  }
}