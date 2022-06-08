using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Tic_Tac_Toe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<Player, ImageSource> imageSources = new()
        {
            {Player.X, new BitmapImage(new Uri("pack://application:,,,/Assets/X15.png")) },
            {Player.O, new BitmapImage(new Uri("pack://application:,,,/Assets/O15.png")) }
        };

        private readonly Dictionary<Player, ObjectAnimationUsingKeyFrames> animations = new()
        {
            {Player.X, new ObjectAnimationUsingKeyFrames() },
            {Player.O, new ObjectAnimationUsingKeyFrames() },
        };

        private readonly DoubleAnimation fadeOutAnimation = new()
        {
            Duration = TimeSpan.FromMilliseconds(480),
            From = 1,
            To = 0
        };

        private readonly DoubleAnimation fadeInAnimation = new()
        {
            Duration = TimeSpan.FromMilliseconds(480),
            From = 0,
            To = 1
    };

    private readonly Image[,] imageControls = new Image[3, 3];
        private readonly GameState gameState = new();

        public MainWindow()
        {
            InitializeComponent();
            SetupGameGrid();
            InitAnimations();

            gameState.MoveMade += OnMoveMade;
            gameState.GameEnded += OnGameEnded;
            gameState.GameRestarted += OnGameRestarted;
        }

        private void SetupGameGrid()
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    Image imageControl = new();
                    GameGrid.Children.Add(imageControl);
                    imageControls[r, c] = imageControl;
                }
            }
        }

        private void InitAnimations()
        {
            animations[Player.X].Duration = TimeSpan.FromMilliseconds(250);
            animations[Player.O].Duration = TimeSpan.FromMilliseconds(250);

            for (int i = 0; i < 16; i++)
            {
                Uri xUri = new($"pack://application:,,,/Assets/X{i}.png");
                BitmapImage xImage = new(xUri);
                DiscreteObjectKeyFrame xKeyframe = new(xImage);
                animations[Player.X].KeyFrames.Add(xKeyframe);

                Uri oUri = new($"pack://application:,,,/Assets/O{i}.png");
                BitmapImage oImage = new(oUri);
                DiscreteObjectKeyFrame oKeyframe = new(oImage);
                animations[Player.O].KeyFrames.Add(oKeyframe);
            }
        }

        private async Task FadeOut(UIElement e)
        {
            e.BeginAnimation(OpacityProperty, fadeOutAnimation);
            await Task.Delay(fadeOutAnimation.Duration.TimeSpan);
            e.Visibility = Visibility.Hidden;
        }

        private async Task FadeIn(UIElement e)
        {
            e.BeginAnimation(OpacityProperty, fadeInAnimation);
            await Task.Delay(fadeInAnimation.Duration.TimeSpan);
            e.Visibility = Visibility.Visible;
        }

        private async Task TransitionToGameScreen()
        {
            //EndScreen.Visibility = Visibility.Hidden;
            await FadeOut(EndScreen);
            //TurnPanel.Visibility = Visibility.Visible;
            //GameCanvas.Visibility = Visibility.Visible;
            Line.Visibility = Visibility.Hidden;
            await Task.WhenAll(FadeIn(TurnPanel), FadeIn(GameCanvas));
        }

        private async Task TransitionToEndScreen(string text, ImageSource winnerImage)
        {
            //TurnPanel.Visibility = Visibility.Hidden;
            //GameCanvas.Visibility = Visibility.Hidden;
            await Task.WhenAll(FadeOut(TurnPanel), FadeOut(GameCanvas));
            ResultText.Text = text;
            WinnerImage.Source = winnerImage;
            //EndScreen.Visibility = Visibility.Visible;
            await FadeIn(EndScreen);
        }

        private (Point,Point) FindLinePoints(WinInfo winInfo)
        {
            double squareSize = GameGrid.Width / 3;
            double margin = squareSize / 2;

            if(winInfo.Type == WinType.Row)
            {
                double y = (winInfo.Number * squareSize) + margin;
                return (new Point(0, y), new Point(GameGrid.Width, y));
            }
            if(winInfo.Type == WinType.Column)
            {
                double x = (winInfo.Number * squareSize) + margin;
                return (new Point(x, 0), new Point(x, GameGrid.Height));
            }
            if (winInfo.Type == WinType.Diagonal)
            {
                return (new Point(0, 0), new Point(GameGrid.Width, GameGrid.Height));
            }

            return (new Point(GameGrid.Width, 0), new Point(0, GameGrid.Height) );
        }

        private async Task ShowLine(WinInfo winInfo)
        {
            (Point start, Point end) = FindLinePoints(winInfo);

            Line.X1 = start.X;
            Line.Y1 = start.Y;

            //Line.X2 = end.X;
            DoubleAnimation xAnimation = new()
            {
                Duration = TimeSpan.FromMilliseconds(300),
                From = start.X,
                To = end.X,
            };
            //Line.Y2 = end.Y;
            DoubleAnimation yAnimation = new()
            {
                Duration = TimeSpan.FromMilliseconds(300),
                From = start.Y,
                To = end.Y,
            };

            Line.Visibility = Visibility.Visible;
            Line.BeginAnimation(Line.X2Property, xAnimation);
            Line.BeginAnimation(Line.Y2Property, yAnimation);
            await Task.Delay(xAnimation.Duration.TimeSpan);
        }

        private void OnMoveMade(int r, int c)
        {
            Player player = gameState.Grid[r, c];
            //imageControls[r, c].Source = imageSources[player];
            imageControls[r, c].BeginAnimation(Image.SourceProperty, animations[player]);
            PlayerImage.Source = imageSources[gameState.CurrentPlayer];
        }

        private async void OnGameEnded(GameResult gameResult)
        {
            await Task.Delay(300);
            if(gameResult.Winner == Player.None)
            {
                await TransitionToEndScreen("Its a tie", null!);
            }
            else
            {
                await ShowLine(gameResult.WinInfo);
                await Task.Delay(1500);
                await TransitionToEndScreen(" wins", imageSources[gameResult.Winner]);
            }
        }

        private async void OnGameRestarted()
        {
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    imageControls[r, c].BeginAnimation(Image.SourceProperty, null);
                    imageControls[r, c].Source = null;
                }
            }

            PlayerImage.Source = imageSources[gameState.CurrentPlayer];
            await TransitionToGameScreen();
        }

        private void GameGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double squareSize = GameGrid.Width / 3;
            Point clickPosition = e.GetPosition(GameGrid);
            int row = (int)(clickPosition.Y / squareSize);
            int col = (int)(clickPosition.X / squareSize);
            gameState.MakeMove(row, col);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(gameState.GameOver)
                gameState.Reset();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}