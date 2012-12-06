#region File Information
/*
 * Space Game (it's a working title)
 *  Copyright (C) 2012 Big Fat Soda Company
 *
 * MoveList.cs - A list of moves...
 * A list of moves that is optimised for efficient matching.
 */
#endregion

#region Imports
using System.Collections.Generic;
using System.Linq;
using SpaceGame.Managers;
#endregion

namespace SpaceGame.InputComponents
{
    public class MoveList
    {
        #region Fields & properties
        // The internal array of moves
        private Move[] moves;
        #endregion

        #region Initialisation
        public MoveList(IEnumerable<Move> moves)
        {
            // Store the list of moves in order of decreasing sequence length.
            // This greatly simplifies the logic of the DetectMove method.
            this.moves = moves.OrderByDescending(m => m.Sequence.Length).ToArray();
        }
        #endregion

        /// <summary>
        /// Finds the longest Move which matches the given input, if any.
        /// </summary>
        public Move DetectMove(int player, InputManager input)
        {
            // Perform a linear search for a move which matches the input. This relies
            // on the moves array being in order of decreasing sequence length.
            foreach (Move move in moves)
            {
                if (input.Matches(player, move))
                {
                    return move;
                }
            }
            return null;
        }

        public int LongestMoveLength
        {
            get
            {
                // Since they are in decreasing order,
                // the first move is the longest.
                return moves[0].Sequence.Length;
            }
        }
    }
}
