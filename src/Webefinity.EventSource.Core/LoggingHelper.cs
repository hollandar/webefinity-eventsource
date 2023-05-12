﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webefinity.EventSource.Core
{

    /// <summary>
    /// Various logging functions to speed up logging of non-enabled log levels.
    /// </summary>
    public static class LoggingHelper
    {
        /// <summary>
        /// Generate a log format with constuction delayed.
        /// </summary>
        /// <param name="msg">The message</param>
        /// <param name="param">Parameters to go into the message.</param>
        /// <returns></returns>
        public static Func<(string message, object?[]? parameters)> LogFormat(this string msg, params object?[] param)
        {
            return () => (msg, param);
        }

        /// <summary>
        /// Generate a log format with constuction delayed.
        /// </summary>
        /// <param name="msg">The message</param>
        /// <param name="param">A function that is able to return a parameter.</param>
        /// <returns></returns>
        public static Func<(string message, object?[]? parameters)> LogFormat(this string msg, Func<object?> param)
        {
            return () => (msg, new object?[] { param() });
        }

        /// <summary>
        /// Generate a log format with constuction delayed.
        /// </summary>
        /// <param name="msg">The message</param>
        /// <param name="param">A function that returns multiple parameters.</param>
        /// <returns></returns>
        public static Func<(string message, object?[]? parameters)> LogFormat(this string msg, Func<object?[]> param)
        {
            return () => (msg, param());
        }

        /// <summary>
        /// Log a level as usual, but with log parameter derivation and the logging itself delayed until after we know
        /// we want to log the level.
        /// </summary>
        /// <param name="logger">A logger instance.</param>
        /// <param name="level">The level to log.</param>
        /// <param name="msg">A func that returns the message and its parameters.</param>
        public static void FastLog(this ILogger? logger, LogLevel level, Func<(string msg, object?[]? parameters)> msg)
        {
            if (logger?.IsEnabled(level) ?? false)
            {
                var props = msg();
                logger.Log(level, props.msg, props.parameters ?? Array.Empty<object>());
            }
        }

        /// <summary>
        /// Log an excaption as usual, but with log parameter derivation and the logging itself delayed until after we know
        /// we want to log the level.
        /// </summary>
        /// <param name="logger">A logger instance.</param>
        /// <param name="level">The level to log.</param>
        /// <param name="e">The exception.</param>
        /// <param name="msg">A func that returns the message and its parameters.</param>
        public static void FastLogException(this ILogger? logger, LogLevel level, Exception e, Func<(string msg, object?[]? parameters)>? msg = null)
        {
            if (logger?.IsEnabled(level) ?? false)
            {
                if (msg != null)
                {
                    var props = msg();
                    logger.Log(level, e, props.msg, props.parameters ?? Array.Empty<object>());
                }
                else
                {
                    logger.Log(level, e, String.Empty);
                }
            }
        }
    }
}
