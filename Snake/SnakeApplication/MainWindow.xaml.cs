using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SnakeApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        # region Fields

        /// <summary>
        /// The score.
        /// </summary>
        private int score;

        /// <summary>
        /// The snake length.
        /// </summary>
        private int snakeLength;

        /// <summary>
        /// Lenght of game mesh lenght (N x N).
        /// </summary>
        private int meshLength;

        /// <summary>
        /// Direction of snake move.
        /// </summary>
        private Direction direction;

        /// <summary>
        /// The apple.
        /// </summary>
        private Rectangle apple;

        /// <summary>
        /// List of snake body parts.
        /// </summary>
        private List<Rectangle> snake;

        /// <summary>
        /// Dispatcher timer.
        /// </summary>
        private DispatcherTimer dispatcherTimer;

        #endregion

        #region Enums

        /// <summary>
        /// Enum - direction.
        /// </summary>
        private enum Direction
        {
            Up = 0,
            Down = 1,
            Right = 2,
            Left = 3
        }

        #endregion

        #region Constructor

        /// <summary>
        /// MainWindow constructior.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            this.meshLength = 20;
            this.snake = new List<Rectangle>();

            this.CreateMesh();

            this.dispatcherTimer = new DispatcherTimer();
            this.dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            this.dispatcherTimer.Interval = new TimeSpan(1 * 1000 * 1000); // 0.1s

            this.SetGame();
            this.dispatcherTimer.Start();
        }

        #endregion

        #region Events

        /// <summary>
        /// Dispatcher timer tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!this.MoveSnake())
            {
                this.dispatcherTimer.Stop();
                this.GameOver();
                this.dispatcherTimer.Start();
            }
        }

        /// <summary>
        /// Key event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void KeyEvent(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    this.SetDirection(Direction.Up);
                    break;
                case Key.Down:
                    this.SetDirection(Direction.Down);
                    break;
                case Key.Right:
                    this.SetDirection(Direction.Right);
                    break;
                case Key.Left:
                    this.SetDirection(Direction.Left);
                    break;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create mesh (rows x columns).
        /// </summary>
        private void CreateMesh()
        {
            for (int i = 0; i < this.meshLength; i++)
            {
                this.gameGrid.RowDefinitions.Add(new RowDefinition());
                this.gameGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }

        /// <summary>
        /// Set game.
        /// </summary>
        private void SetGame()
        {
            this.score = 0;
            this.snakeLength = 3;
            this.direction = Direction.Up;
            this.labelScore.Content = score;

            this.gameGrid.Children.Remove(this.apple);

            foreach (var snakePart in this.snake)
            {
                this.gameGrid.Children.Remove(snakePart);
            }

            this.snake = CreateSnake();
            this.apple = CreateApple();

            this.SetImage();
        }

        /// <summary>
        /// Game over.
        /// </summary>
        private void GameOver()
        {
            this.SetGame();
        }

        /// <summary>
        /// Set snake direction move.
        /// </summary>
        /// <param name="direction">Snake move direction.</param>
        private void SetDirection(Direction direction)
        {
            if (this.direction == Direction.Up && direction == Direction.Down) return;
            if (this.direction == Direction.Down && direction == Direction.Up) return;
            if (this.direction == Direction.Right && direction == Direction.Left) return;
            if (this.direction == Direction.Left && direction == Direction.Right) return;

            this.direction = direction;
        }

        /// <summary>
        /// Move snake.
        /// </summary>
        /// <returns>True if snake will move, otherwise false (collision).</returns>
        private bool MoveSnake()
        {
            int headRow = Grid.GetRow(this.snake[0]);
            int headColumn = Grid.GetColumn(this.snake[0]);

            switch (this.direction)
            {
                case Direction.Up:
                    headRow--;
                    break;
                case Direction.Down:
                    headRow++;
                    break;
                case Direction.Right:
                    headColumn++;
                    break;
                case Direction.Left:
                    headColumn--;
                    break;
            }

            if (headRow > this.meshLength - 1)
                headRow = 0;
            else if (headRow < 0)
                headRow = this.meshLength - 1;

            if (headColumn > this.meshLength - 1)
                headColumn = 0;
            else if (headColumn < 0)
                headColumn = this.meshLength - 1;

            if (this.CheckCollision(headRow, headColumn))
            {
                return false;
            }

            if (!this.EatApple(headRow, headColumn))
            {
                this.gameGrid.Children.Remove(this.snake[this.snakeLength - 1]);
                this.snake.Remove(this.snake[this.snakeLength - 1]);
            }

            Rectangle rectangle = this.AddRectangle(headRow, headColumn);
            this.snake.Insert(0, rectangle);

            return true;
        }

        /// <summary>
        /// Check collision of snake head with snake body.
        /// </summary>
        /// <param name="headRow">Position of snake head - row.</param>
        /// <param name="headColumn">Position of snake head - column.</param>
        /// <returns>True if there will be a snake collision, else false.</returns>
        private bool CheckCollision(int headRow, int headColumn)
        {
            foreach (var snakePart in this.snake)
            {
                if (headRow == Grid.GetRow(snakePart) && headColumn == Grid.GetColumn(snakePart))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Eat apple.
        /// </summary>
        /// <param name="headRow">Position of snake head - row.</param>
        /// <param name="headColumn">Position of snake head - column.</param>
        /// <returns>True if apple is eaten, else false.</returns>
        private bool EatApple(int headRow, int headColumn)
        {
            if (headRow != Grid.GetRow(this.apple) || headColumn != Grid.GetColumn(this.apple))
            {
                return false;
            }

            this.score++;
            this.snakeLength++;
            this.labelScore.Content = score;

            this.gameGrid.Children.Remove(this.apple);
            this.apple = this.CreateApple();

            this.SetImage();

            return true;
        }

        /// <summary>
        /// Add rectangle (snake part).
        /// </summary>
        /// <param name="row">Row position of rectangle.</param>
        /// <param name="column">Column position of rectangle.</param>
        /// <returns>The rectangle.</returns>
        private Rectangle AddRectangle(int row, int column)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Fill = Brushes.Black;

            Grid.SetRow(rectangle, row);
            Grid.SetColumn(rectangle, column);

            this.gameGrid.Children.Add(rectangle);
            return rectangle;
        }

        /// <summary>
        /// Create snake.
        /// </summary>
        /// <returns>Snake.</returns>
        private List<Rectangle> CreateSnake()
        {
            Rectangle rectangle;
            List<Rectangle> snake = new List<Rectangle>();

            int meshCenter = this.meshLength / 2;

            for (int i = 0; i < this.snakeLength; i++)
            {
                rectangle = this.AddRectangle(meshCenter + i, meshCenter);
                snake.Add(rectangle);
            }

            return snake;
        }

        /// <summary>
        /// Create apple.
        /// </summary>
        /// <returns>Apple.</returns>
        private Rectangle CreateApple()
        {
            int appleRow;
            int appleColumn;
            Random rng = new Random();
            Rectangle apple;

            do
            {
                appleRow = rng.Next(0, this.meshLength - 1);
                appleColumn = rng.Next(0, this.meshLength - 1);
            }
            while (this.CheckCollision(appleRow, appleColumn));

            apple = this.AddRectangle(appleRow, appleColumn);
            apple.Fill = Brushes.Red;

            return apple;
        }

        /// <summary>
        /// Set new image.
        /// </summary>
        private void SetImage()
        {
            string imageUrl = "https://cdn.pixabay.com/photo/";
            string url1 = "2015/02/28/15/25/snake-653639_960_720.jpg";
            string url2 = "2016/08/31/18/19/snake-1634293_960_720.jpg";
            string url3 = "2014/11/23/21/22/green-tree-python-543243_960_720.jpg";
            string url4 = "2015/10/30/15/04/green-tree-python-1014229_960_720.jpg";
            string url5 = "2019/02/06/17/09/snake-3979601_960_720.jpg";
            string[] urls = new string[] { url1, url2, url3, url4, url5 };

            Random rng = new Random();
            BitmapImage bitmap = new BitmapImage();

            imageUrl += urls[rng.Next(0, urls.Length)];

            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imageUrl);
            bitmap.EndInit();

            gameImage.Source = bitmap;
        }

        #endregion
    }
}
