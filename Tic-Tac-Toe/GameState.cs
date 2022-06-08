using System;

namespace Tic_Tac_Toe
{
    internal class GameState
    {
        public Player[,] Grid { get; private set; }
        public Player CurrentPlayer { get; private set; }
        public int TurnsPassed { get; private set; }
        public bool GameOver { get; private set; }

        public event Action<int, int>? MoveMade;

        public event Action<GameResult>? GameEnded;

        public event Action? GameRestarted;

        public GameState()
        {
            Grid = new Player[3, 3];
            CurrentPlayer = Player.X;
            TurnsPassed = 0;
            GameOver = false;
        }

        private bool CanMakeMove(int r, int c)
        {
            return !GameOver && Grid[r, c] == Player.None;
        }

        private bool IsGridFull()
        {
            return TurnsPassed == 9;
        }

        private void SwitchPlayer()
        {
            CurrentPlayer = CurrentPlayer == Player.X ? Player.O : Player.X;
        }

        private bool AreSquareMarked((int, int)[] squares, Player player)
        {
            foreach ((int r, int c) in squares)
            {
                if (Grid[r, c] != player)
                {
                    return false;
                }
            }

            return true;
        }

        private bool DidMoveWin(int r, int c, out WinInfo winInfo)
        {
            (int, int)[] row = new[] { (r, 0), (r, 1), (r, 2) };
            (int, int)[] column = new[] { (0, c), (1, c), (2, c) };
            (int, int)[] diag = new[] { (0, 0), (1, 1), (2, 2) };
            (int, int)[] antiDiag = new[] { (0, 2), (1, 1), (2, 0) };

            if (AreSquareMarked(row, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.Row, Number = r };
                return true;
            }

            if (AreSquareMarked(column, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.Column, Number = c };
                return true;
            }

            if (AreSquareMarked(diag, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.Diagonal };
                return true;
            }

            if (AreSquareMarked(antiDiag, CurrentPlayer))
            {
                winInfo = new WinInfo { Type = WinType.AntiDiagonal };
                return true;
            }

            winInfo = null!;
            return false;
        }

        private bool DidMoveEndGame(int r, int c, out GameResult gameResult)
        {
            if (DidMoveWin(r, c, out WinInfo winInfo))
            {
                gameResult = new GameResult { Winner = CurrentPlayer, WinInfo = winInfo };
                return true;
            }

            if (IsGridFull())
            {
                gameResult = new GameResult { Winner = Player.None };
                return true;
            }

            gameResult = null!;
            return false;
        }

        public void MakeMove(int r, int c)
        {
            if (!CanMakeMove(r, c))
            {
                return;
            }

            Grid[r, c] = CurrentPlayer;
            TurnsPassed++;

            if (DidMoveEndGame(r, c, out GameResult gameResult))
            {
                GameOver = true;
                MoveMade?.Invoke(r, c);
                GameEnded?.Invoke(gameResult);
            }
            else
            {
                SwitchPlayer();
                MoveMade?.Invoke(r, c);
            }
        }

        public void Reset()
        {
            Grid = new Player[3, 3];
            CurrentPlayer = Player.X;
            TurnsPassed = 0;
            GameOver = false;
            GameRestarted?.Invoke();
        }
    }
}