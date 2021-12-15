using System;
using System.Drawing;
using System.Windows.Forms;
using static RT.Legacy;

namespace RT
{
    class RayTracing
    {
        public static Point3D Camera;

        public static Color RayCast(Point3D  point, Point3D  dir, int depth = 0) 
        {
            Point3D normal = new Point3D();
            Point3D intersectPoint = new Point3D();
            Material material = new Material();

            bool intersect = false;
            double dist = double.MaxValue;

            foreach (Sphere sphere in scene.Spheres)
            {
                double a = 0;
                if (depth < 3 && sphere.RayIntersect(point, dir, ref a))
                {
                    if (a < dist)
                    {
                        intersect = true;

                        intersectPoint = point + dir * a;
                        normal = Normalize(intersectPoint - sphere.location);
                        dist = a;

                        material = sphere.mat;
                    }
                }
            }

            foreach (Mesh mesh in scene.Cubes)
            {
                foreach (Polygon pol in mesh.faces)
                {
                    double a = 0;
                    Point3D zeroPoint = new Point3D();
                    if (depth < 3 && pol.RayIntersect(point, dir, ref zeroPoint, ref a))
                    {
                        if (a < dist)
                        {
                            intersect = true;

                            dist = a;
                            intersectPoint = zeroPoint;
                            normal = pol.normal;
                            material = mesh.mat;
                        }
                    }
                }
            }

            if (intersect)
            {
                return RetrieveColor(dir, depth, normal, intersectPoint, material);
            }

            return Color.FromArgb(0, 0, 0);
        }

        private static Color RetrieveColor(Point3D dir, int depth, Point3D normal, Point3D intersectPoint, Material material)
        {
            double specularIntensity = 0;
            double diffuseIntensity = 0;

            Color reflect_color = new Color();
            Color refract_color = new Color();

            foreach (Light light in scene.lights)
            {
                Point3D lightVecFull = light.location - intersectPoint;
                Point3D lightVecN = Normalize(lightVecFull);
                double light_distance = Length(lightVecFull);

                Point3D shadow_point = lightVecN * normal < 0 ? intersectPoint - normal * 0.001 : intersectPoint + normal * 0.001;
                Point3D shadow_hit = new Point3D();
                Point3D shadow_N = new Point3D();
                Material mat_s = new Material();

                if (material.retractParam == 0)
                {
                    if (GetIntersectionPoint(shadow_point, lightVecN, ref shadow_hit, ref shadow_N, ref mat_s) && Length(shadow_hit - shadow_point) < light_distance)
                    {
                        continue;
                    }
                }

                diffuseIntensity += light.intensity * Math.Max(0.0, (lightVecN * normal));
                specularIntensity += Math.Pow(Math.Max(0.0, Reflection(lightVecN, normal) * dir), material.specular) * light.intensity;

                if (material.reflectParam != 0)
                {
                    Point3D reflectionDirect = Normalize(Reflection(dir, normal));
                    Point3D reflect_orig = reflectionDirect * normal < 0 ? intersectPoint - normal * 0.001 : intersectPoint + normal * 0.001;

                    reflect_color = RayCast(reflect_orig, reflectionDirect, depth + 1);
                }

                if (material.retractParam != 0)
                {
                    Point3D retractionDirect = Normalize(Retraction(dir, normal, material.retractionIndex));
                    Point3D refract_orig = retractionDirect * normal < 0 ? intersectPoint - normal * 0.001 : intersectPoint + normal * 0.001;

                    refract_color = RayCast(refract_orig, retractionDirect, depth + 1);
                }
            }

            double diffuseR = 0;
            double diffuseG = 0;
            double diffuseB = 0;
            double specularR = 0;
            double specularG = 0;
            double specularB = 0;
            double reflectR = 0;
            double reflectG = 0;
            double reflectB = 0;
            double retractR = 0;
            double retractG = 0;
            double retractB = 0;

            if (material.diffusParam != 0)
            {
                diffuseR = material.diffus.R * diffuseIntensity;
                diffuseG = material.diffus.G * diffuseIntensity;
                diffuseB = material.diffus.B * diffuseIntensity;
            }

            if (material.specularParam != 0)
            {
                specularR = material.diffus.R * diffuseIntensity * material.diffusParam * specularIntensity * material.specularParam;
                specularG = material.diffus.G * diffuseIntensity * material.diffusParam * specularIntensity * material.specularParam;
                specularB = material.diffus.B * diffuseIntensity * material.diffusParam * specularIntensity * material.specularParam;
            }

            if (material.reflectParam != 0)
            {
                reflectR = reflect_color.R * material.reflectParam;
                reflectG = reflect_color.G * material.reflectParam;
                reflectB = reflect_color.B * material.reflectParam;
            }

            if (material.retractParam != 0)
            {
                retractR = refract_color.R * material.retractParam;
                retractG = refract_color.G * material.retractParam;
                retractB = refract_color.B * material.retractParam;
            }

            int r = (int)(diffuseR + specularR + reflectR + retractR);
            int g = (int)(diffuseG + specularG + reflectG + retractG);
            int b = (int)(diffuseB + specularB + reflectB + retractB);

            r = r > 255 ? 255 : r;
            g = g > 255 ? 255 : g;
            b = b > 255 ? 255 : b;

            return Color.FromArgb(r, g, b);
        }

        public static void RenderFunc(Bitmap bmp, PictureBox picture)
        {
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    int x = i - bmp.Width / 2;
                    int y = j - bmp.Height / 2;
                    int z = (int)(-Camera.Z);

                    double distance = Distance(new Point3D(x, y, z), new Point3D(0, 0, 0));

                    bmp.SetPixel(i, j, RayCast(Camera, new Point3D(x / distance, y / distance, z / distance)));
                }
            }

            picture.Refresh();
        }
    }
}
