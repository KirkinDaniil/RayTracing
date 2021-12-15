using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static RT.Legacy;
using static RT.RayTracing;
using static RT.Athens;

namespace RT
{
    public partial class Form1 : Form
    {
        Bitmap bmp;
        static bool sphereTransparent = false;
        static bool cubeTransparent = false;
        static bool sphereSpecular = false;
        static bool cubeSpecular = false;
        static Point3D secondLightPos = new Point3D(-150, 0, 0);
        static List<bool> specularWalls = new List<bool>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DrawRoom();
        }

        private void DrawRoom()
        {
            Camera = new Point3D(0, 0, -500, 0);
            bmp = new Bitmap(RenderTarget.Width, RenderTarget.Height);

            RenderTarget.Image = bmp;
            scene = new Scene();

            LoadScene();
            LoadRoom();

            RenderFunc(bmp, RenderTarget);
        }

        public static void LoadScene()
        {
            if (scene.Cubes.Count != 0 && scene.Spheres.Count != 0)
            {
                scene.Cubes.Clear();
                scene.Spheres.Clear();
            }
            var firstCube = new Mesh();
            LoadMesh(ref firstCube, "../../models/cube.obj");
            firstCube.mat = new Material();
            firstCube.mat.diffus = Color.Gray;
            firstCube.mat.diffusParam = 0.9;
            firstCube.mat.specularParam = 0.8;
            firstCube.mat.reflectParam = 0.0;
            firstCube.mat.retractParam = 0.0;
            if (cubeTransparent)
            {
                firstCube.mat.diffusParam = 0.0;
                firstCube.mat.specularParam = 0.0;
                firstCube.mat.retractParam = 0.9;
                firstCube.mat.retractionIndex = 1.3;
            }
            else if (cubeSpecular)
            {
                firstCube.mat.diffusParam = 0.0;
                firstCube.mat.specularParam = 0.0;
                firstCube.mat.reflectParam = 1.0;
            }
            AtheneTransform(ref firstCube, AtheneScale(0.4, 0.4, 0.4));
            AtheneTransform(ref firstCube, AtheneRotate(30.0 / 360.0 * (2.0 * Math.PI), 'y'), true);
            AtheneTransform(ref firstCube, AtheneMove(0, 120, 0));
            scene.Cubes.Add(firstCube);

            var secondCube = new Mesh();
            LoadMesh(ref secondCube, "../../models/cube.obj");
            secondCube.mat = new Material();
            secondCube.mat.diffus = Color.Red;
            secondCube.mat.diffusParam = 0.9;
            secondCube.mat.specularParam = 0.8;
            secondCube.mat.reflectParam = 0.0;
            secondCube.mat.retractParam = 0.0;
            AtheneTransform(ref secondCube, AtheneScale(0.4, 0.4, 0.4));
            AtheneTransform(ref secondCube, AtheneRotate(30.0 / 360.0 * (2.0 * Math.PI), 'y'), true);
            AtheneTransform(ref secondCube, AtheneMove(100, 120, -120));
            scene.Cubes.Add(secondCube);

            var sphere = new Sphere();
            sphere.mat = new Material();
            sphere.mat.diffus = Color.White;
            sphere.mat.retractionIndex = 1.3;
            sphere.mat.diffusParam = 1.0;
            sphere.mat.specularParam = 1.0;
            sphere.mat.reflectParam = 0.0;
            sphere.mat.retractParam = 0.0;

            if (sphereTransparent)
            {
                sphere.mat.diffusParam = 0.0;
                sphere.mat.specularParam = 0.0;
                sphere.mat.retractParam = 0.9;
            }
            else if (sphereSpecular)
            {
                sphere.mat.diffusParam = 0.0;
                sphere.mat.specularParam = 0.0;
                sphere.mat.reflectParam = 1.0;
            }
            sphere.location = new Point3D(90, 35, -40);
            sphere.radius = 40;
            scene.Spheres.Add(sphere);

            var secondLight = new Light();
            secondLight.location = new Point3D(0, 0, -400);
            secondLight.intensity = 0.5;
            secondLight.color = Color.White;
            scene.lights.Add(secondLight);

            var firstLight = new Light();
            firstLight.location = secondLightPos;
            firstLight.intensity = 0.3;
            firstLight.color = Color.White;
            scene.lights.Add(firstLight);
        }

        private void LoadRoom()
        {
            var floor = new Mesh();
            LoadMesh(ref floor, "../../models/plane.obj");
            floor.mat = new Material();
            floor.mat.diffus = Color.Aqua;
            floor.mat.diffusParam = 1.0;
            floor.mat.specularParam = 0.0;
            floor.mat.reflectParam = 0.0;
            floor.mat.retractParam = 0.0;
            AtheneTransform(ref floor, AtheneScale(2.5, 2.5, 5.0));
            AtheneTransform(ref floor, AtheneRotate((180) / 360.0 * (2.0 * Math.PI), 'z'), true);
            AtheneTransform(ref floor, AtheneMove(0, -100 + 250, -1));
            if (specularWalls.Count != 0 && specularWalls[4])
            {
                floor.mat.diffusParam = 0.0;
                floor.mat.reflectParam = 1.0;
            }
            scene.Cubes.Add(floor);

            var ceiling = new Mesh();
            LoadMesh(ref ceiling, "../../models/plane.obj");
            ceiling.mat = new Material();
            ceiling.mat.diffus = Color.Orange;
            ceiling.mat.diffusParam = 1.0;
            ceiling.mat.specularParam = 0.0;
            ceiling.mat.reflectParam = 0.0;
            ceiling.mat.retractParam = 0.0;
            AtheneTransform(ref ceiling, AtheneScale(2.5, 2.5, 5.0));
            AtheneTransform(ref ceiling, AtheneMove(0, -100 - 250, -1));
            if (specularWalls.Count != 0 && specularWalls[3])
            {
                ceiling.mat.diffusParam = 0.0;
                ceiling.mat.reflectParam = 1.0;
            }
            scene.Cubes.Add(ceiling);

            var leftWall = new Mesh();
            LoadMesh(ref leftWall, "../../models/plane.obj");
            leftWall.mat = new Material();
            leftWall.mat.diffus = Color.White;
            leftWall.mat.diffusParam = 1.0;
            leftWall.mat.specularParam = 0.0;
            leftWall.mat.reflectParam = 0.0;
            leftWall.mat.retractParam = 0.0;
            AtheneTransform(ref leftWall, AtheneScale(2.5, 2.5, 5.0));
            AtheneTransform(ref leftWall, AtheneRotate((90) / 360.0 * (2.0 * Math.PI), 'z'), true);
            AtheneTransform(ref leftWall, AtheneMove(-250, -100, -1));
            if (specularWalls.Count != 0 && specularWalls[0])
            {
                leftWall.mat.diffusParam = 0.0;
                leftWall.mat.reflectParam = 1.0;
            }
            scene.Cubes.Add(leftWall);

            var frontWall = new Mesh();
            LoadMesh(ref frontWall, "../../models/plane.obj");
            frontWall.mat = new Material();
            frontWall.mat.diffus = Color.Red;
            frontWall.mat.diffusParam = 1.0;
            frontWall.mat.specularParam = 0.0;
            frontWall.mat.reflectParam = 0.0;
            frontWall.mat.retractParam = 0.0;
            AtheneTransform(ref frontWall, AtheneScale(2.5, 2.5, 2.5));
            AtheneTransform(ref frontWall, AtheneRotate((90) / 360.0 * (2.0 * Math.PI), 'x'), true);
            AtheneTransform(ref frontWall, AtheneMove(0, -100, -1 + 500));
            if (specularWalls.Count != 0 && specularWalls[1])
            {
                frontWall.mat.diffusParam = 0.0;
                frontWall.mat.reflectParam = 1.0;
            }
            scene.Cubes.Add(frontWall);

            var rightWall = new Mesh();
            LoadMesh(ref rightWall, "../../models/plane.obj");
            rightWall.mat = new Material();
            rightWall.mat.diffus = Color.Green;
            rightWall.mat.specular = 100;
            rightWall.mat.diffusParam = 1.0;
            rightWall.mat.specularParam = 0.0;
            rightWall.mat.reflectParam = 0.0;
            rightWall.mat.retractParam = 0.0;
            AtheneTransform(ref rightWall, AtheneScale(2.5, 2.5, 5.0));
            AtheneTransform(ref rightWall, AtheneRotate((-90) / 360.0 * (2.0 * Math.PI), 'z'), true);
            AtheneTransform(ref rightWall, AtheneMove(250, -100, -1));
            if (specularWalls.Count != 0 && specularWalls[2])
            {
                rightWall.mat.diffusParam = 0.0;
                rightWall.mat.reflectParam = 1.0;
            }
            scene.Cubes.Add(rightWall);

            var backWall = new Mesh();
            LoadMesh(ref backWall, "../../models/plane.obj");
            backWall.mat = new Material();
            backWall.mat.diffus = Color.AliceBlue;
            backWall.mat.diffusParam = 1.0;
            backWall.mat.specularParam = 0.0;
            backWall.mat.reflectParam = 0.0;
            backWall.mat.retractParam = 0.0;
            AtheneTransform(ref backWall, AtheneScale(2.5, 2.5, 2.5));
            AtheneTransform(ref backWall, AtheneRotate((-90) / 360.0 * (2.0 * Math.PI), 'x'), true);
            AtheneTransform(ref backWall, AtheneMove(0, -100, -500 - 1));
            scene.Cubes.Add(backWall);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sphereTransparent = checkBox2.Checked;
            cubeTransparent = checkBox1.Checked;
            cubeSpecular = checkBox4.Checked;
            sphereSpecular = checkBox3.Checked;
            DrawRoom();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string coords = textBox1.Text;
            string[] xyz = coords.Split(';');
            int x = Convert.ToInt32(xyz[0]);
            int y = Convert.ToInt32(xyz[1]);
            int z = Convert.ToInt32(xyz[2]);
            secondLightPos = new Point3D(x, y, z);
            DrawRoom();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            specularWalls.Clear();
            specularWalls.Add(checkBox8.Checked);
            specularWalls.Add(checkBox7.Checked);
            specularWalls.Add(checkBox6.Checked);
            specularWalls.Add(checkBox5.Checked);
            specularWalls.Add(checkBox9.Checked);
            DrawRoom();
        }
    }
}
