﻿/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hl7.Fhir.Rest;

namespace Hl7.Fhir.Tests.Model
{
    [TestClass]
#if PORTABLE45
	public class PortableBundleExtensionsTest
#else
	public class BundleExtensionsTest
#endif
    {
        [TestMethod]
        public void TestFindEntry()
        {
            Bundle b = new Bundle();
            b.AddResourceEntry(new Patient { Id = "4" }, "http://some.org/fhir/Patient/4");
            b.AddResourceEntry(new Patient { Id = "5", Meta = new Meta { VersionId = "5" } }, "http://some.org/fhir/Patient/5");
            b.AddResourceEntry(new Patient { Id = "6" }, "http://some.org/fhir/Patient/8");

            Assert.AreEqual(1, b.FindEntry("http://some.org/fhir/Patient/4").Count());
            Assert.AreEqual(1, b.FindEntry("http://some.org/fhir/Patient/5").Count());
            Assert.AreEqual(0, b.FindEntry("http://some.org/fhir/Patient/6").Count());
            Assert.AreEqual(1, b.FindEntry("http://some.org/fhir/Patient/8").Count());

            Assert.AreEqual(1, b.FindEntry("http://some.org/fhir/Patient/5/_history/5").Count());
            Assert.AreEqual(0, b.FindEntry("http://some.org/fhir/Patient/5/_history/6").Count());

            Assert.AreEqual(1, b.FindEntry("https://some.org/fhir/Patient/4").Count());
        }


        [TestMethod]
        public void ResourceListFiltering()
        {
            var testBundle = new Bundle();

            testBundle.Entry.Add(new Bundle.BundleEntryComponent { Resource = new Patient { Id = "1234", Meta = new Meta { VersionId = "v2" } } });
            testBundle.Entry.Add(new Bundle.BundleEntryComponent { Resource = new Patient { Id = "1234", Meta = new Meta { VersionId = "v3" } } });
            testBundle.Entry.Add(new Bundle.BundleEntryComponent { Resource = new Patient { Id = "1234", Meta = new Meta { VersionId = "v4"} }, 
                            Request = new Bundle.BundleEntryRequestComponent {Method = Bundle.HTTPVerb.DELETE}  });

            testBundle.Entry.Add(new Bundle.BundleEntryComponent { Resource = new Patient { Id = "5678" }, FullUrl = "http://server1.com/fhir/Patient/5678" });
            testBundle.Entry.Add(new Bundle.BundleEntryComponent { Resource = new Patient { Id = "1.2.3.4.5" }, FullUrl = "urn:oid:1.2.3.4.5" });

            var result = testBundle.FindEntry("Patient", "1234");
            Assert.AreEqual(2, result.Count());
            result = testBundle.FindEntry("Patient", "1234", includeDeleted: true);
            Assert.AreEqual(3, result.Count());
            result = testBundle.FindEntry("Patient", "1234", "v3", includeDeleted: true);
            Assert.AreEqual(1, result.Count());
            result = testBundle.FindEntry(new Uri("http://server3.org/fhir/Patient/1234"));
            Assert.AreEqual(0, result.Count());

            result = testBundle.FindEntry("Patient", "5678");
            Assert.AreEqual(1, result.Count());
            result = testBundle.FindEntry(new Uri("http://server1.com/fhir/Patient/5678"));
            Assert.AreEqual(1, result.Count());
            result = testBundle.FindEntry(new Uri("http://server2.com/fhir/Patient/5678"));
            Assert.AreEqual(0, result.Count());

            result = testBundle.FindEntry(new Uri("urn:oid:1.2.3.4.5"));
            Assert.AreEqual(1, result.Count());
        }
    }
}
