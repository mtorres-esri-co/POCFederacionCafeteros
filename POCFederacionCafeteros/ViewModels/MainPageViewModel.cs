using System.Windows.Input;

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;

using Newtonsoft.Json;

using Drawing = System.Drawing;
using GeoJsonFeature = GeoJSON.Net.Feature;
using Mapping = Esri.ArcGISRuntime.Mapping;

namespace POCFederacionCafeteros.ViewModels
{
  internal class MainPageViewModel : BindableBase
  {
    private Mapping.Map _map;
    private GraphicsOverlayCollection _graphicOverlyaCollection;
    private Viewpoint _newViewpoint;
    private Viewpoint _actualViewpoint;
    private string _title;

    /// <summary>
    /// 
    /// </summary>
    public string Title
    {
      get => _title;
      set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public Mapping.Map Map
    {
      get => _map;
      set => SetProperty(ref _map, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public GraphicsOverlayCollection GraphicsOverlayCollection
    {
      get => _graphicOverlyaCollection;
      set => SetProperty(ref _graphicOverlyaCollection, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public Viewpoint NewViewpoint
    {
      get => _newViewpoint;
      set => SetProperty(ref _newViewpoint, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public Viewpoint ActualViewpoint
    {
      get => _actualViewpoint;
      set => SetProperty(ref _actualViewpoint, value);
    }

    /// <summary>
    /// 
    /// </summary>
    public ICommand UpdateViewpointCommand { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public ICommand LoadJsonCommand { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    private MapPoint CentralPoint { get; set; }

    /// <summary>
    /// 
    /// </summary>
    private FeatureCollectionLayer? FeatureCollectionLayer { get; set; }

    /// <summary>
    /// 
    /// </summary>
    private FeatureCollection? FeatureCollection { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="navigationService"></param>
    public MainPageViewModel(INavigationService navigationService)
        : base()
    {
      Title = "Demo Nugets EsriDevSummit Colombia";

      Map = new Mapping.Map(BasemapStyle.ArcGISStreets);
      GraphicsOverlayCollection =
      [
        new GraphicsOverlay() { Id = "Hotel Cosmos 100"},
        new GraphicsOverlay() { Id = "Eventos"}
      ];
      CentralPoint = new MapPoint(-74.054424, 4.685715, SpatialReferences.Wgs84);

      var graphic = new Graphic()
      {
        Geometry = CentralPoint,
        Symbol = new SimpleMarkerSymbol
        {
          Color = Drawing.Color.DarkGreen,
          Style = SimpleMarkerSymbolStyle.Diamond,
          Size = 20
        }
      };

      var textGraphic = new Graphic()
      {
        Geometry = CentralPoint,
        Symbol = new TextSymbol
        {
          Text = "Hotel Cosmos 100",
          Color = Drawing.Color.DarkGreen,
          Size = 15,
          OffsetY = 20
        }
      };

      GraphicsOverlayCollection[0].Graphics.Add(graphic);
      GraphicsOverlayCollection[0].Graphics.Add(textGraphic);
      var viewpoint = new Viewpoint(CentralPoint, 5000);

      Map.InitialViewpoint = viewpoint;
      NewViewpoint = Map.InitialViewpoint;

      UpdateViewpointCommand = new DelegateCommand<Viewpoint>(UpdateViewpoint);
      LoadJsonCommand = new DelegateCommand(async () => await LoadJsonActionAsync());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="viewpoint"></param>
    private void UpdateViewpoint(Viewpoint viewpoint)
    {
      if(viewpoint != null)
      {
        ActualViewpoint = viewpoint.TargetGeometry is MapPoint ?
        new Viewpoint(
            viewpoint.TargetGeometry.Project(SpatialReferences.Wgs84) as MapPoint,
            viewpoint.TargetScale) :
          new Viewpoint(
            viewpoint.TargetGeometry.Project(SpatialReferences.Wgs84),
            viewpoint.Camera);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private async Task LoadJsonActionAsync()
    {
      var folder = FileSystem.AppDataDirectory;
      var files = Directory.GetFiles(folder, "*.geojson", SearchOption.AllDirectories);
      FeatureCollection = FeatureCollection is null ? new FeatureCollection() : FeatureCollection;

      foreach(var file in files)
      {
        var layerName = Path.GetFileNameWithoutExtension(file);
        var rendererFileName = Path.Combine(folder, $"{layerName}.lyrx");
        var rendererJson = File.Exists(rendererFileName) ? await File.ReadAllTextAsync(rendererFileName) : null;
        if(rendererJson is not null)
        {
          var renderer = Renderer.FromJson(rendererJson);
          Console.WriteLine(renderer);
        }
        var geoJson = await File.ReadAllTextAsync(file);
        await FromGeoJson(layerName, geoJson);
      }

      if(FeatureCollection.Tables.Count > 0)
      {
        FeatureCollectionLayer = new FeatureCollectionLayer(FeatureCollection);
        await FeatureCollectionLayer.LoadAsync();

        Map.OperationalLayers.Add(FeatureCollectionLayer);

        var featureLayer = FeatureCollectionLayer.Layers[0];
        if(featureLayer is not null && featureLayer.FullExtent is not null)
        {
          NewViewpoint = new Viewpoint(featureLayer.FullExtent);
        }
        var outJson2 = FeatureCollection.ToJson();

        await File.WriteAllTextAsync(Path.Combine(folder, "out2.json"), outJson2);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="geoJson"></param>
    /// <returns></returns>
    private async Task FromGeoJson(string layerName, string geoJson)
    {
      var geoJsonFeatureCollection = JsonConvert.DeserializeObject<GeoJsonFeature.FeatureCollection>(geoJson);
      var geoJsonGeometryType = geoJsonFeatureCollection.Features[0].Geometry.Type;
      var geometryType = GeometryType.Unknown;

      switch(geoJsonGeometryType)
      {
        case GeoJSON.Net.GeoJSONObjectType.Point:
        case GeoJSON.Net.GeoJSONObjectType.MultiPoint:
          geometryType = GeometryType.Point;

          break;
        case GeoJSON.Net.GeoJSONObjectType.LineString:
        case GeoJSON.Net.GeoJSONObjectType.MultiLineString:
          geometryType = GeometryType.Polyline;
          break;
        case GeoJSON.Net.GeoJSONObjectType.Polygon:
        case GeoJSON.Net.GeoJSONObjectType.MultiPolygon:
          geometryType = GeometryType.Polygon;
          break;
        default:
          break;
      }

      if(FeatureCollection is not null)
      {
        var featureCollectionTable = FeatureCollection.Tables.FirstOrDefault(t => t.DisplayName == layerName);
        if(featureCollectionTable is null)
        {
          featureCollectionTable = new FeatureCollectionTable([], geometryType, SpatialReferences.Wgs84)
          {
            DisplayName = layerName,
            Renderer = MainPageViewModelHelpers.CreateRenderer(geometryType)
          };
          FeatureCollection.Tables.Add(featureCollectionTable);
        }

        foreach(var feature in geoJsonFeatureCollection.Features)
        {
          var attributes = feature.Properties;
          var geometry = feature.Geometry;
          switch(geometry.Type)
          {
            case GeoJSON.Net.GeoJSONObjectType.Point:
            case GeoJSON.Net.GeoJSONObjectType.MultiPoint:
              var point = geometry as GeoJSON.Net.Geometry.Point;
              if(point is not null)
              {
                var mapPoint = new MapPoint(point.Coordinates.Longitude, point.Coordinates.Latitude, SpatialReferences.Wgs84);
                var pointFeature = featureCollectionTable.CreateFeature(attributes, mapPoint);
                await featureCollectionTable.AddFeatureAsync(pointFeature);
              }
              break;

            case GeoJSON.Net.GeoJSONObjectType.LineString:
              var lineString = geometry as GeoJSON.Net.Geometry.LineString;
              if(lineString is not null)
              {
                var polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
                foreach(var coordinate in lineString.Coordinates)
                {
                  polylineBuilder.AddPoint(new MapPoint(coordinate.Longitude, coordinate.Latitude, SpatialReferences.Wgs84));
                }
                var polyline = polylineBuilder.ToGeometry();
                var polylineFeature = featureCollectionTable.CreateFeature(attributes, polyline);
                await featureCollectionTable.AddFeatureAsync(polylineFeature);
              }
              break;

            case GeoJSON.Net.GeoJSONObjectType.Polygon:
              var polygon = geometry as GeoJSON.Net.Geometry.Polygon;
              if(polygon is not null)
              {
                var polygonBuilder = new PolygonBuilder(SpatialReferences.Wgs84);
                foreach(var ring in polygon.Coordinates)
                {
                  var part = new Esri.ArcGISRuntime.Geometry.PointCollection(SpatialReferences.Wgs84);
                  foreach(var coordinate in ring.Coordinates)
                  {
                    part.Add(new MapPoint(coordinate.Longitude, coordinate.Latitude, SpatialReferences.Wgs84));
                  }
                  polygonBuilder.AddPart(part);
                }
                var outPolygon = polygonBuilder.ToGeometry();
                var polygonFeature = featureCollectionTable.CreateFeature(attributes, outPolygon);
                await featureCollectionTable.AddFeatureAsync(polygonFeature);
              }
              break;

            default:
              break;
          }
        }
      }
    }
  }
}
