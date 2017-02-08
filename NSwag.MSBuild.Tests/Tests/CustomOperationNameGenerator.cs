using System;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace NSwag.MSBuild.Tests.Tests
{
    public class CustomOperationNameGenerator : IOperationNameGenerator
    {
        #region IOperationNameGenerator Members

        /// <summary>
        ///     Gets the client name for a given operation.
        /// </summary>
        /// <param name="document">The Swagger document.</param>
        /// <param name="path">The HTTP path.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="operation">The operation.</param>
        /// <returns>
        ///     The client name.
        /// </returns>
        public string GetClientName(SwaggerDocument document, string path, SwaggerOperationMethod httpMethod, SwaggerOperation operation)
        {
            return "HelloClient";
        }

        /// <summary>
        ///     Gets the operation name for a given operation.
        /// </summary>
        /// <param name="document">The Swagger document.</param>
        /// <param name="path">The HTTP path.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <param name="operation">The operation.</param>
        /// <returns>
        ///     The operation name.
        /// </returns>
        public string GetOperationName(SwaggerDocument document, string path, SwaggerOperationMethod httpMethod, SwaggerOperation operation)
        {
            return "assadasdasd";
        }

        /// <summary>
        ///     Gets a value indicating whether the generator supports multiple client classes.
        /// </summary>
        public bool SupportsMultipleClients
        {
            get { return false; }
        }

        #endregion
    }
}