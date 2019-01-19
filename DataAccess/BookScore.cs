﻿namespace DataAccess
{
    /// <summary>
    ///     A book score.
    /// </summary>
    public class BookScore
    {
        /// <summary>
        ///     Gets or sets the identifier of the book.
        /// </summary>
        /// <value>
        ///     The identifier of the book.
        /// </value>

        public string BookId { get; set; }

        /// <summary>
        ///     Gets or sets the rate.
        /// </summary>
        /// <value>
        ///     The rate.
        /// </value>

        public short Rate { get; set; }

        /// <summary>
        ///     Gets or sets the predicted rate.
        /// </summary>
        /// <value>
        ///     The predicted rate.
        /// </value>

        public double PredictedRate { get; set; }
        public int UserId { get; set; }
    }
}