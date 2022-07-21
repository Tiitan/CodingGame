using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Numerics;
using System.IO;
using System.Collections.Generic;

namespace MarsLander
{
    using SDColor = System.Drawing.Color;
    using SWMColor = System.Windows.Media.Color;

    public static class ColorExt
    {

        public static SWMColor ToSWMColor(this SDColor color) => SWMColor.FromArgb(color.A, color.R, color.G, color.B);
        public static SDColor ToSDColor(this SWMColor color) => SDColor.FromArgb(color.A, color.R, color.G, color.B);
    }

    /// <summary>
    /// WPF path viewer
    /// </summary>
    public partial class MainWindow : Window
    {
        bool inputTextDirty = false; // input text changed = save input on next Run
        List<Vector2>? path = null; // Path computed in a task and re-used in mouse move event to display mouse position projection onto the path

        public MainWindow()
        {
            InitializeComponent();

            // Try load saved map when window open (app start)
            if (File.Exists("InputText.txt"))
            {
                InputText.Text = File.ReadAllText("InputText.txt");
                OnRun(this, new RoutedEventArgs());
            }
        }

        SolidColorBrush TerrainLineBrush = new SolidColorBrush(System.Drawing.Color.Black.ToSWMColor());
        SolidColorBrush PathLineBrush = new SolidColorBrush(System.Drawing.Color.Blue.ToSWMColor());
        SolidColorBrush BlockBrush = new SolidColorBrush(System.Drawing.Color.DarkRed.ToSWMColor());
        SolidColorBrush CloseBlock = new SolidColorBrush(System.Drawing.Color.LightPink.ToSWMColor());
        SolidColorBrush StartBrush = new SolidColorBrush(System.Drawing.Color.DarkGreen.ToSWMColor());
        SolidColorBrush FinishBrush = new SolidColorBrush(System.Drawing.Color.DarkBlue.ToSWMColor());
        SolidColorBrush DistanceToPathBrush = new SolidColorBrush(System.Drawing.Color.DarkViolet.ToSWMColor());

        // Draw a line, passed in world coordinate 
        void DrawLine(Canvas canvas, Vector2 a, Vector2 b, SolidColorBrush color, double scale = 0.2f)
        {
            Line line = new Line();

            line.Stroke = color;
            line.X1 = a.X * scale;
            line.Y1 = (3000 - a.Y) * scale;
            line.X2 = b.X * scale;
            line.Y2 = (3000 - b.Y) * scale;

            canvas.Children.Add(line);
        }

        // "Fill" a grid cell, passed in grid coordinate
        void DrawCell(Vector2i pos, SolidColorBrush color, float gridResolution, float scale = 0.2f)
        {
            Rectangle rect = new Rectangle();
            float cellSize = gridResolution * scale;
            rect.Fill = color;
            rect.Height = cellSize;
            rect.Width = cellSize;
            Canvas.SetLeft(rect, pos.X * cellSize) ;
            Canvas.SetTop(rect, ((3000 / gridResolution) - pos.Y) * cellSize - cellSize);

            MainCanvas.Children.Add(rect);
        }

        // Run button pressed (or startup): rebuild map and compute path then draw everything
        private void OnRun(object sender, RoutedEventArgs e)
        {

            string textMap = InputText.Text;
            int gridResolution = 100;
            int.TryParse(GridResolution.Text, out gridResolution);
            Game? game = null;
            Lander? lander = null;
            bool drawGrid = GridCheckbox.IsChecked == true;

            // Save map for convenient app restart
            if (inputTextDirty)
                Task.Run(() => File.WriteAllText("InputText.txt", textMap));

            Task initTask = Task.Run(() => // map initialization on a background thread
            {
                var inputStream = new StringReader(textMap);
                game = new Game(inputStream, gridResolution);
                lander = new Lander(inputStream);
            });
            initTask.ContinueWith(_ => { // Draw map on UI thread
                MainCanvas.Children.Clear();
                // Map cells
                if (drawGrid)
                {
                    for (int i = 0; i < game!.gridMap.GetLength(0); i++)
                        for (int j = 0; j < game.gridMap.GetLength(1); j++)
                            if (game.gridMap[i, j].Type != 0)
                                DrawCell(new Vector2i(i, j), game.gridMap[i, j].Type == 2 ? BlockBrush : CloseBlock, game.GridResolution);
                }

                // Terrain vector
                for (int i = 0; i < game!.surfaceN - 1; i++)
                    DrawLine(MainCanvas, game.vectorTerrain[i], game.vectorTerrain[i + 1], TerrainLineBrush);

                DrawCell(game.FromWorldCoordinate(lander!.position), StartBrush, game.GridResolution);
                DrawCell(game.FromWorldCoordinate(game.Finish), FinishBrush, game.GridResolution);
            }, TaskScheduler.FromCurrentSynchronizationContext());

            // Path
            if (PathCheckbox.IsChecked == true)
            {
                initTask.ContinueWith(_ => // Pathfinding on a background thread
                {
                    path = game!.BuildPath(lander!.position);
                }).ContinueWith(_ => // Draw path on UI thread
                {
                    if (path != null)
                        for (int i = 0; i < path.Count - 1; i++)
                            DrawLine(MainCanvas, path[i], path[i + 1], PathLineBrush);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        // Map updated (saved and used on next Run)
        private void OnInputTextChanged(object sender, RoutedEventArgs e)
        {
            inputTextDirty = true;
        }

        // Mouse moved over canvas: draw mouse projection onto the path.
        private void DynamicCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            DynamicCanvas.Children.Clear();
            if (path == null) 
                return;
            Point mousePosition = e.GetPosition(sender as IInputElement);
            Vector2 worldPosition = new Vector2((float)mousePosition.X, (600 - (float)mousePosition.Y)) * 5;

            LogLabel.Content = $"mousePosition={mousePosition} worldPosition={worldPosition}";

            float minDistance = float.PositiveInfinity;
            Vector2 minPositionOnSegment = worldPosition;
            for (int i = 0; i < path.Count - 1; i++)
            {

                (float progress, Vector2 positionOnSegment) = Controller.GetPositionOnSegment(path[i], path[i + 1], worldPosition);
                float distance = Vector2.Distance(positionOnSegment, worldPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    minPositionOnSegment = positionOnSegment;
                }
            }
            DrawLine(DynamicCanvas, worldPosition, minPositionOnSegment, DistanceToPathBrush);
        }
    }
}
