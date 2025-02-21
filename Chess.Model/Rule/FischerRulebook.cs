//-----------------------------------------------------------------------
// <copyright file="StandardRulebook.cs">
//     Copyright (c) Michael Szvetits. All rights reserved.
// </copyright>
// <author>Michael Szvetits</author>
//-----------------------------------------------------------------------
namespace Chess.Model.Rule
{
    using Chess.Model.Command;
    using Chess.Model.Data;
    using Chess.Model.Game;
    using Chess.Model.Piece;
    using Chess.Model.Visitor;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Numerics;
    using System;

    /// <summary>
    /// Represents the standard chess rulebook.
    /// </summary>
    public class FischerRulebook : IRulebook
    {
        /// <summary>
        /// Represents the check rule of a standard chess game.
        /// </summary>
        private readonly CheckRule checkRule;

        /// <summary>
        /// Represents the end rule of a standard chess game.
        /// </summary>
        private readonly EndRule endRule;

        /// <summary>
        /// Represents the movement rule of a standard chess game.
        /// </summary>
        private readonly MovementRule movementRule;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardRulebook"/> class.
        /// </summary>
        public FischerRulebook()
        {
            var threatAnalyzer = new ThreatAnalyzer();
            var castlingRule = new CastlingRule(threatAnalyzer);
            var enPassantRule = new EnPassantRule();
            var promotionRule = new PromotionRule();

            this.checkRule = new CheckRule(threatAnalyzer);
            this.movementRule = new MovementRule(castlingRule, enPassantRule, promotionRule, threatAnalyzer);
            this.endRule = new EndRule(this.checkRule, this.movementRule);
        }

        /// <summary>
        /// Creates a new chess game according to the Chess960 rulebook.
        /// </summary>
        /// <returns>The newly created chess game.</returns>
        public ChessGame CreateGame()
        {
            List<int> placements = new List<int>();

            void randomBaseLine()
            {
                Random random = new Random();
                List<int> darkSquares = new List<int> { 0, 2, 4, 6 };
                List<int> lightSquares = new List<int> { 1, 3, 5, 7 };

                placements.Add(darkSquares[random.Next(darkSquares.Count)]);
                placements.Add(lightSquares[random.Next(lightSquares.Count)]);
                List<int> availablePositions = Enumerable.Range(0, 8).Except(new List<int> { placements[0], placements[1] }).ToList();

                placements.Add(availablePositions[random.Next(availablePositions.Count)]);
                availablePositions.Remove(placements[2]);

                placements.Add(availablePositions[random.Next(availablePositions.Count)]);
                availablePositions.Remove(placements[3]);

                placements.Add(availablePositions[random.Next(availablePositions.Count)]);
                availablePositions.Remove(placements[4]);

                placements.Add(availablePositions[0]);

                placements.Add(availablePositions[1]);

                placements.Add(availablePositions[2]);
            }

            void reverseBaseLine()
            {
                for (int i = 0; i < placements.Count; i++)
                {
                    placements[i] = 7 - placements[i];
                }
            }

            IEnumerable<PlacedPiece> makeBaseLine(int row, Color color)
            {
                yield return new PlacedPiece(new Position(row, placements[5]), new Rook(color));
                yield return new PlacedPiece(new Position(row, placements[2]), new Knight(color));
                yield return new PlacedPiece(new Position(row, placements[0]), new Bishop(color));
                yield return new PlacedPiece(new Position(row, placements[4]), new Queen(color));
                yield return new PlacedPiece(new Position(row, placements[6]), new King(color));
                yield return new PlacedPiece(new Position(row, placements[1]), new Bishop(color));
                yield return new PlacedPiece(new Position(row, placements[3]), new Knight(color));
                yield return new PlacedPiece(new Position(row, placements[7]), new Rook(color));
            }

            IEnumerable<PlacedPiece> makePawns(int row, Color color) =>
                Enumerable.Range(0, 8).Select(
                    i => new PlacedPiece(new Position(row, i), new Pawn(color))
                );

            IImmutableDictionary<Position, ChessPiece> makePieces(int pawnRow, int baseRow, Color color)
            {
                var pawns = makePawns(pawnRow, color);
                var baseLine = makeBaseLine(baseRow, color);
                var pieces = baseLine.Union(pawns);
                var empty = ImmutableSortedDictionary.Create<Position, ChessPiece>(PositionComparer.DefaultComparer);
                return pieces.Aggregate(empty, (s, p) => s.Add(p.Position, p.Piece));
            }
            randomBaseLine();
            var whitePlayer = new Player(Color.White);
            var whitePieces = makePieces(1, 0, Color.White);

            reverseBaseLine();
            var blackPlayer = new Player(Color.Black);
            var blackPieces = makePieces(6, 7, Color.Black);
            var board = new Board(whitePieces.AddRange(blackPieces));

            return new ChessGame(board, whitePlayer, blackPlayer);
        }

        /// <summary>
        /// Gets the status of a chess game, according to the standard rulebook.
        /// </summary>
        /// <param name="game">The game state to be analyzed.</param>
        /// <returns>The current status of the game.</returns>
        public Status GetStatus(ChessGame game)
        {
            return this.endRule.GetStatus(game);
        }

        /// <summary>
        /// Gets all possible updates (i.e., future game states) for a chess piece on a specified position,
        /// according to the standard rulebook.
        /// </summary>
        /// <param name="game">The current game state.</param>
        /// <param name="position">The position to be analyzed.</param>
        /// <returns>A sequence of all possible updates for a chess piece on the specified position.</returns>
        public IEnumerable<Update> GetUpdates(ChessGame game, Position position)
        {
            var piece = game.Board.GetPiece(position, game.ActivePlayer.Color);
            var updates = piece.Map(
                p =>
                {
                    var moves = this.movementRule.GetCommands(game, p);
                    var turnEnds = moves.Select(c => new SequenceCommand(c, EndTurnCommand.Instance));
                    var records = turnEnds.Select
                    (
                        c => new SequenceCommand(c, new SetLastUpdateCommand(new Update(game, c)))
                    );
                    var futures = records.Select(c => c.Execute(game).Map(g => new Update(g, c)));
                    return futures.FilterMaybes().Where
                    (
                        e => !this.checkRule.Check(e.Game, e.Game.PassivePlayer)
                    );
                }
            );

            return updates.GetOrElse(Enumerable.Empty<Update>());
        }
    }
}