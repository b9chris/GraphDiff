using System.Collections.ObjectModel;
using System.Linq;
using System.Data.Entity;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RefactorThis.GraphDiff.Tests.Models;

namespace RefactorThis.GraphDiff.Tests.Tests
{
	[TestClass]
	public class SimpleBehaviours : TestBase
	{
		/// <summary>
		/// Adds a very simple model with none of the special parts EFGraphDiff is necessary for and checks if it's updated
		/// properly - a potential blindside for the library.
		/// </summary>
		[TestMethod]
		public void ShouldUpdateSimpleModel()
		{
			var model = new SimpleModel();
			model.Name = "One";
			using (var context = new TestDbContext())
			{
				context.SimpleModels.Add(model);
				context.SaveChanges();
			}

			int id = model.Id;
			model.Name = "Two";
			using (var context = new TestDbContext())
			{
				context.UpdateGraph(model);
				context.SaveChanges();
			}

			using(var context = new TestDbContext())
			{
				var reloadedEntity = context.SimpleModels.Find(id);
				Assert.AreEqual("Two", reloadedEntity.Name);
			}
		}

		/// <summary>
		/// Adds a simple model with a minor twist - a ComplexType as some of its columns.
		/// Simulates the effect of submitting an update via JSON POST on MVC, which creates a new, detached POCO with no EF
		/// plumbing and passes it as-is to EFGraphDiff to update in the db.
		/// </summary>
		[TestMethod]
		public void ShouldUpdateModelWithComplexType()
		{
			var model = new PersonWithAddress();
			model.Name = "One";
			model.Address = new Address();
			model.Address.City = "Boston";
			model.Address.State = "MA";
			using (var context = new TestDbContext())
			{
				context.PeopleWithAddresses.Add(model);
				context.SaveChanges();
			}

			int id = model.Id;

			// Simulate a fresh, detached model coming in over JSON through an MVC POST
			model = new PersonWithAddress
			{
				Id = id,
				Name = "Two",
				Address = new Address
				{
					City = "Cambridge",
					State = "MA"
				}
			};

			using (var context = new TestDbContext())
			{
				context.UpdateGraph(model);
				context.SaveChanges();
			}

			using(var context = new TestDbContext())
			{
				var reloadedEntity = context.PeopleWithAddresses.Find(id);
				Assert.AreEqual("Two", reloadedEntity.Name);
				Assert.AreEqual("Cambridge", reloadedEntity.Address.City);
			}
		}
	}
}
