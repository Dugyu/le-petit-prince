using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { /* Implementation hidden. */ }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { /* Implementation hidden. */ }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { /* Implementation hidden. */ }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private readonly RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private readonly GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private readonly IGH_Component Component;
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private readonly int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments,
  /// Output parameters as ref arguments. You don't have to assign output parameters,
  /// they will have a default value.
  /// </summary>
  private void RunScript(Point3d light, Polyline boundary, Point3d light2, Polyline boundary2, Point3d cornerA, Point3d cornerB, Mesh wall, int xCount, int yCount, int zCount, ref object A, ref object B, ref object C)
  {

    List <Point3d> pts = getGridPoints(cornerA, cornerB, xCount, yCount, zCount);

    double dx = 0.5 * (cornerB.X - cornerA.X) / (xCount - 1.0);
    double dy = 0.5 * (cornerB.Y - cornerA.Y) / (yCount - 1.0);
    double dz = 0.5 * (cornerB.Z - cornerA.Z) / (zCount - 1.0);


    Mesh shadow = Rhino.Geometry.Mesh.CreateFromClosedPolyline(boundary);
    Mesh shadow2 = Rhino.Geometry.Mesh.CreateFromClosedPolyline(boundary2);

    List <Point3d> volumePoints = new List <Point3d> ();
    List <Mesh> volume = new List<Mesh>();
    List <Point3d> shadowPoints = new List <Point3d> ();
    for (int i = 0; i < pts.Count; i++)
    {
      Point3d p = pts[i];
      Vector3d lightDirection = p - light;
      Ray3d ray = new Ray3d(light, lightDirection);

      double alongTheRay = Rhino.Geometry.Intersect.Intersection.MeshRay(shadow, ray);
      if (alongTheRay > 0.0)
      {
        Vector3d lightDirection2 = p - light2;
        Ray3d ray2 = new Ray3d(light2, lightDirection2);


        double alongTheRay2 = Rhino.Geometry.Intersect.Intersection.MeshRay(shadow2, ray2);
        if (alongTheRay2 > 0.0)
        {

          double wallPoint = Rhino.Geometry.Intersect.Intersection.MeshRay(wall, ray);
          shadowPoints.Add(ray.PointAt(wallPoint));
          double wallPoint2 = Rhino.Geometry.Intersect.Intersection.MeshRay(wall, ray2);
          shadowPoints.Add(ray2.PointAt(wallPoint2));

          volumePoints.Add(p);
          Mesh cube = getGridCube(p, dx, dy, dz);
          volume.Add(cube);

        }
      }
    }

    A = volumePoints;
    B = volume;
    C = shadowPoints;


  }

  // <Custom additional code> 

  public List <Point3d> getGridPoints(Point3d cornerA, Point3d cornerB, int xCount, int yCount, int zCount)
  {
    List<Point3d> points = new List<Point3d>();

    double x0 = cornerA.X;
    double y0 = cornerA.Y;
    double z0 = cornerA.Z;

    double x1 = cornerB.X;
    double y1 = cornerB.Y;
    double z1 = cornerB.Z;

    double dx = (x1 - x0) / (xCount - 1.0);
    double dy = (y1 - y0) / (yCount - 1.0);
    double dz = (z1 - z0) / (zCount - 1.0);

    for (int k = 0; k < zCount; ++k)
    {
      double z = z0 + k * dz;
      for (int j = 0; j < yCount; ++j)
      {
        double y = y0 + j * dy;
        for (int i = 0; i < xCount; ++i)
        {
          double x = x0 + i * dx;
          Point3d p = new Point3d(x, y, z);
          points.Add(p);
        }
      }
    }

    return points;

  }

  public Mesh getGridCube(Point3d p, double x, double y, double z)

  {

    Point3d p1 = (new Point3d(-x, -y, -z)) + p;
    Point3d p2 = (new Point3d(x, -y, -z)) + p;
    Point3d p3 = (new Point3d(x, -y, z)) + p;
    Point3d p4 = (new Point3d(-x, -y, z)) + p;
    Point3d p5 = (new Point3d(-x, y, z)) + p;
    Point3d p6 = (new Point3d(x, y, z)) + p;
    Point3d p7 = (new Point3d(x, y, -z)) + p;
    Point3d p8 = (new Point3d(-x, y, -z)) + p;

    Mesh cube = new Mesh();

    cube.Vertices.Add(p1);
    cube.Vertices.Add(p2);
    cube.Vertices.Add(p3);
    cube.Vertices.Add(p4);
    cube.Vertices.Add(p5);
    cube.Vertices.Add(p6);
    cube.Vertices.Add(p7);
    cube.Vertices.Add(p8);
    cube.Faces.AddFace(0, 1, 2, 3);
    cube.Faces.AddFace(1, 6, 5, 2);
    cube.Faces.AddFace(6, 7, 4, 5);
    cube.Faces.AddFace(7, 0, 3, 4);
    cube.Faces.AddFace(2, 5, 4, 3);
    cube.Faces.AddFace(0, 7, 6, 1);


    return cube;
  }


  // </Custom additional code> 
}