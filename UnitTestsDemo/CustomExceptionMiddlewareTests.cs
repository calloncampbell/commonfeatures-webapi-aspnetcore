using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using WebApiDemo;
using WebApiDemo.Middlewares;
using WebApiDemo.Models;
using Xunit;

namespace UnitTestsDemo
{
    public class CustomExceptionMiddlewareTests
    {
        [Fact]
        public async Task WhenACustomExceptionIsRaised_CustomExceptionMiddlewareShouldHandleItToCustomErrorResponse()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<CustomExceptionMiddleware>>();
            var middleware = new CustomExceptionMiddleware((innerHttpContext) =>
            {
                throw new ModelValidationException(new List<string>() { "Error1, Error2" });
            }, loggerMock);

            var context = new DefaultHttpContext();

            // Initialize response body
            context.Response.Body = new MemoryStream();

            //Act
            await middleware.Invoke(context);

            // set the position to beginning before reading
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            // Read the response
            var reader = new StreamReader(context.Response.Body);
            var streamText = reader.ReadToEnd();
            var objResponse = JsonConvert.DeserializeObject<Error>(streamText);

            //Assert
            objResponse
            .Should()
            .BeEquivalentTo(new { Message = "Validation errors", Errors = new List<string>() { "Error1, Error2" }, Code = "00001" });

            context.Response.StatusCode
            .Should()
            .Be((int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task WhenAGenericExceptionIsRaised_CustomExceptionMiddlewareShouldHandleItToDefaultErrorResponse()
        {
            // Arrange
            var loggerMock = Substitute.For<ILogger<CustomExceptionMiddleware>>();
            var middleware = new CustomExceptionMiddleware((innerHttpContext) =>
            {
                throw new Exception("Oooops error!");
            }, loggerMock);

            var context = new DefaultHttpContext();

            // Initialize response body
            context.Response.Body = new MemoryStream();

            //Act
            await middleware.Invoke(context);

            // set the position to beginning before reading
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            // Read the response
            var reader = new StreamReader(context.Response.Body);
            var streamText = reader.ReadToEnd();
            var objResponse = JsonConvert.DeserializeObject<Error>(streamText);

            //Assert
            objResponse
            .Should()
            .BeEquivalentTo(new { Message = "Unhandled error", Errors = new List<string>() , Code = "00009" });

            context.Response.StatusCode
            .Should()
            .Be((int)HttpStatusCode.InternalServerError);
        }
    }
}
