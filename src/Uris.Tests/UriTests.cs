using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Immutable;
using System.Linq;

#pragma warning disable CA1055 // Url-like return values should not be strings
#pragma warning disable IDE0057 // Use range operator
#pragma warning disable IDE0050 // Convert to tuple
#pragma warning disable CA1305 // Specify IFormatProvider

namespace Urls.UnitTests
{
    [TestClass]
    public class UrlTests
    {
        private const string Scheme = "http";
        private const string Host = "host.com";
        private const int Port = 5000;
        private const string PathPart1 = "pathpart1";
        private const string PathPart2 = "pathpart2";
        private const string FieldName1 = "fieldname1";
        private const string FieldName2 = "FieldName2";
        private const string FieldValue1 = "field<>Value1";
        private const string FieldValue2 = "field<>Value2";
        private const string FieldValueEncoded1 = "field%3C%3EValue1";
        private const string FieldValueEncoded2 = "field%3C%3EValue2";
        private const string Fragment = "frag";
        private const string Username = "username";
        private const string Password = "password";

        private readonly string expected = $"{Scheme}://{Username}:{Password}@{Host}:{Port}/{PathPart1}/{PathPart2}?" +
            $"{FieldName1}={FieldValueEncoded1}&{FieldName2}={FieldValueEncoded2}#{Fragment}";



        [TestMethod]
        public void TestEquality()
        {
            var absoluteUrl1 = expected.ToAbsoluteUrl();
            Uri uri = absoluteUrl1;
            var absoluteUrl2 = (AbsoluteUrl)uri;

            Assert.AreEqual(uri, absoluteUrl1);
            Assert.AreEqual(uri.ToString(), absoluteUrl2.ToString());
            Assert.AreEqual(absoluteUrl1.ToString(), uri.ToString());
            Assert.AreEqual(absoluteUrl1, absoluteUrl2);
            Assert.AreEqual(absoluteUrl2, uri);
            Assert.AreEqual(absoluteUrl2, absoluteUrl1);
        }

        [TestMethod]
        public void Test()
        {
            var uriString = new AbsoluteUrl(Scheme, Host, Port,
                new RelativeUrl(
                        ImmutableList.Create(PathPart1, PathPart2),
                        ImmutableList.Create(
                            new QueryParameter(FieldName1, FieldValue1),
                            new QueryParameter(FieldName2, FieldValue2)
                            )
                    , Fragment),
                    new UserInfo(Username, Password)).ToString();


            Assert.AreEqual(
                expected,
                uriString);
        }

        [TestMethod]
        public void TestComposition()
        {
            var uri =
                new AbsoluteUrl(Scheme, Host, Port)
                .AddQueryParameter(FieldName1, FieldValue1)
                .WithCredentials(Username, Password)
                .AddQueryParameter(FieldName2, FieldValue2)
                .WithFragment(Fragment)
                .WithPath(PathPart1, PathPart2);

            Assert.AreEqual(
                expected,
                uri.ToString());
        }

        [TestMethod]
        public void TestComposition2()
        {
            var uri =
                Host.ToHttpUriFromHost(Port)
                .AddQueryParameter(FieldName1, FieldValue1)
                .WithCredentials(Username, Password)
                .AddQueryParameter(FieldName2, FieldValue2)
                .WithFragment(Fragment)
                .WithPath(PathPart1, PathPart2);

            Assert.AreEqual(
                expected,
                uri.ToString());
        }

        [TestMethod]
        public void TestComposition3()
        {
            var uri = Host.ToHttpsUriFromHost();

            Assert.AreEqual(
                $"https://{Host}",
                uri.ToString());
        }

        [TestMethod]
        public void TestAbsoluteWithRelative()
        {
            var absolute = new AbsoluteUrl(Scheme, Host);

            var relativeRelativeUrl = new RelativeUrl(
                                    ImmutableList.Create(PathPart1, PathPart2),
                                    ImmutableList.Create(
                                        new QueryParameter(FieldName1, FieldValue1),
                                        new QueryParameter(FieldName2, FieldValue2)
                                        )
                                );

            absolute = absolute.WithRelativeUrl(relativeRelativeUrl);

            Assert.AreEqual(
                relativeRelativeUrl.Fragment,
                absolute.RelativeUrl.Fragment);
        }

        [TestMethod]
        public void TestRelativeWithFragment()
        {
            var relativeRelativeUrl = new RelativeUrl(
                                    ImmutableList.Create(PathPart1, PathPart2),
                                    ImmutableList.Create(
                                        new QueryParameter(FieldName1, FieldValue1),
                                        new QueryParameter(FieldName2, FieldValue2)
                                        )
                                );

            const string frag = "test";

            relativeRelativeUrl = relativeRelativeUrl.WithFragment(frag);

            Assert.AreEqual(
                frag,
                relativeRelativeUrl.Fragment);
        }

        [TestMethod]
        public void TestWithQueryStringStrings()
        {
            var relativeRelativeUrl = RelativeUrl.Empty.AddQueryString(FieldName1, FieldValue1);

            Assert.AreEqual(
            FieldName1,
            relativeRelativeUrl.QueryParameters?.First().FieldName
            );

            Assert.AreEqual(
            FieldValue1,
            relativeRelativeUrl.QueryParameters?.First().Value
            );
        }

        [TestMethod]
        public void TestAbsoluteWithQueryStringStrings()
        {
            var absoluteRelativeUrl = new AbsoluteUrl("https", "test.com");

            absoluteRelativeUrl = absoluteRelativeUrl.AddQueryParameter(FieldName1, FieldValue1);

            Assert.AreEqual(
            FieldName1,
            absoluteRelativeUrl.RelativeUrl.QueryParameters.First().FieldName
            );

            Assert.AreEqual(
            FieldValue1,
            absoluteRelativeUrl.RelativeUrl.QueryParameters.First().Value
            );
        }

        [TestMethod]
        public void TestMinimalAbsoluteToString()
        => Assert.AreEqual("https://test.com", new AbsoluteUrl("https", "test.com").ToString());

        [TestMethod]
        public void TestConstructUri()
        {
            var uriString = new AbsoluteUrl(Scheme, Host, Port,
                new RelativeUrl(
                        ImmutableList.Create(PathPart1, PathPart2)
                        ,
                        ImmutableList.Create(
                            new QueryParameter(FieldName1, FieldValue1),
                            new QueryParameter(FieldName2, FieldValue2)
                            )
                    , Fragment),
                    new UserInfo(Username, Password)).ToString();

            var uri = new Uri(uriString, UriKind.Absolute);

            Assert.IsNotNull(uri);
            Assert.AreEqual(uri.Scheme, Scheme);
        }

        [TestMethod]
        public void TestWithQueryParams()
        {
            var item = new
            {
                somelongstring = "gvhhvhgfgfdg7676878",
                count = 50,
                message = "This is a sentence"
            };

            var relativeUrl = RelativeUrl.Empty.WithQueryParamers(item);

            Assert.AreEqual(item.somelongstring, relativeUrl.QueryParameters[0].Value);
            Assert.AreEqual(nameof(item.somelongstring), relativeUrl.QueryParameters[0].FieldName);
            Assert.AreEqual(item.count.ToString(), relativeUrl.QueryParameters[1].Value);
            Assert.AreEqual(nameof(item.count), relativeUrl.QueryParameters[1].FieldName);
            Assert.AreEqual(item.message, relativeUrl.QueryParameters[2].Value);
            Assert.AreEqual(nameof(item.message), relativeUrl.QueryParameters[2].FieldName);
        }

        [TestMethod]
        public void TestFromUri()
        {
            var uriString = new AbsoluteUrl(Scheme, Host, Port,
                new RelativeUrl(
                        ImmutableList.Create(PathPart1, PathPart2),
                        ImmutableList.Create(
                            new QueryParameter(FieldName1, FieldValue1),
                            new QueryParameter(FieldName2, FieldValue2)
                            )
                    , Fragment),
                    new UserInfo(Username, Password)).ToString();

            var uri = new Uri(uriString, UriKind.Absolute).ToAbsoluteUrl();

            Assert.IsNotNull(uri);
            Assert.AreEqual(uri.Scheme, Scheme);
            Assert.AreEqual(uri.RelativeUrl.Fragment, Fragment);
            Assert.AreEqual(uri.RelativeUrl.QueryParameters.First().FieldName, FieldName1);
            Assert.AreEqual(uri.RelativeUrl.QueryParameters.First().Value, FieldValueEncoded1);
            Assert.AreEqual(uri.RelativeUrl.QueryParameters[1].FieldName, FieldName2);
            Assert.AreEqual(uri.RelativeUrl.QueryParameters[1].Value, FieldValueEncoded2);
            Assert.AreEqual(Host, uri.Host);
            Assert.AreEqual(Port, uri.Port);
            Assert.AreEqual(PathPart1, uri.RelativeUrl.Path[0]);
            Assert.AreEqual(PathPart2, uri.RelativeUrl.Path[1]);
            Assert.AreEqual(Fragment, uri.RelativeUrl.Fragment);
            Assert.AreEqual(Username, uri.UserInfo?.Username);
            Assert.AreEqual(Password, uri.UserInfo?.Password);
        }


        [TestMethod]
        public void TestRelativeUrlConstructors()
        {
            var RelativeUrl = new RelativeUrl("a/a");
            Assert.IsTrue(RelativeUrl.Path.SequenceEqual(new string[] { "a", "a" }));

            RelativeUrl = new RelativeUrl("a/");
            Assert.IsTrue(RelativeUrl.Path.SequenceEqual(new string[] { "a" }));

            RelativeUrl = new RelativeUrl("a/b/c");
            Assert.IsTrue(RelativeUrl.Path.SequenceEqual(new string[] { "a", "b", "c" }));

            RelativeUrl = new RelativeUrl("a/b/c/");
            Assert.IsTrue(RelativeUrl.Path.SequenceEqual(new string[] { "a", "b", "c" }));

            RelativeUrl = new RelativeUrl("");
            Assert.IsTrue(RelativeUrl.Path.SequenceEqual(new string[] { }));
        }



        [TestMethod]
        public void TestToAbsoluteUrlThings()
        {
            var absoluteUrl = new AbsoluteUrl($"{Scheme}://{Host}");

            Assert.AreEqual(Scheme, absoluteUrl.Scheme);
            Assert.AreEqual(Host, absoluteUrl.Host);

            absoluteUrl = new AbsoluteUrl($"{Scheme}://{Host}:{Port}");
            Assert.AreEqual(Port, absoluteUrl.Port);

            absoluteUrl = new AbsoluteUrl("http://www.hotmail.com");
            Assert.AreEqual("www.hotmail.com", absoluteUrl.Host);

            absoluteUrl = new AbsoluteUrl("http://bob:@www.hotmail.com");
            Assert.AreEqual("bob", absoluteUrl.UserInfo.Username);
        }
    }
}


