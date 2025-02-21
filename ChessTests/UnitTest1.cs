using NUnit.Framework;
using Chess.Model.Rule;
using Chess.Model.Game;
using Chess.Model.Piece;
using System.Collections.Generic;
using System.Linq;

namespace ChessTests
{
    public class Tests
    {
        List<PlacedPiece> whitePieces;
        List<PlacedPiece> blackPieces;
        Board board;
        [OneTimeSetUp]
        public void Setup()
        {
            IRulebook rulebook = new FischerRulebook();
            ChessGame game = rulebook.CreateGame();
            board = game.Board;
            whitePieces = board.GetPieces(Color.White).ToList();
            blackPieces = board.GetPieces(Color.Black).ToList();
        }

        [Test]
        public void PieceCount()
        {
            Assert.That(whitePieces.Count, Is.EqualTo(16));
        }

        [Test]
        public void TestBishops()
        {
            List<PlacedPiece> bishops = whitePieces.FindAll(piece => piece.Piece.GetType() == typeof(Bishop));

            Assert.That(IsEven(bishops[0].Position.Column), Is.Not.EqualTo(IsEven(bishops[1].Position.Column)));
        }

        [Test]
        public void TestRooks()
        {
            List<PlacedPiece> rooks = whitePieces.FindAll(piece => piece.Piece.GetType() == typeof(Rook));
            PlacedPiece king = whitePieces.Find(pieces => pieces.Piece.GetType() == typeof(King));
            int kingPosition = king.Position.Column;

            Assert.That(rooks[0].Position.Column < kingPosition, Is.Not.EqualTo(rooks[1].Position.Column < kingPosition));

            Assert.Pass();
        }

        [Test]
        public void TestKingsMirrored()
        {
            PlacedPiece whiteKing = whitePieces.Find(piece => piece.Piece.GetType() == typeof(King));
            PlacedPiece blackKing = blackPieces.Find(piece => piece.Piece.GetType() == typeof(King));
            bool IsOpposite = IsMirroredPosition(whiteKing.Position, blackKing.Position);

            Assert.That(IsOpposite, Is.True);
        }
        public void TestQueensMirrored()
        {
            PlacedPiece whiteKing = whitePieces.Find(piece => piece.Piece.GetType() == typeof(Queen));
            PlacedPiece blackKing = blackPieces.Find(piece => piece.Piece.GetType() == typeof(Queen));
            bool IsOpposite = IsMirroredPosition(whiteKing.Position, blackKing.Position);

            Assert.That(IsOpposite, Is.True);
        }

        public bool IsMirroredPosition(Position position1, Position position2)
        {
            Position mirrorPosition = new Position(7 - position1.Row, 7 - position1.Column);
            return position2.Equals(mirrorPosition);
        }

        public bool IsEven(int number)
        {
            return number % 2 == 0;
        }
    }
}