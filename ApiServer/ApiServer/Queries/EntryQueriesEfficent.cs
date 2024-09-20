using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries
{
	public class EntryQueriesEfficent
	{
		private readonly NbbContext _context;

		public EntryQueriesEfficent(NbbContext context)
		{
			_context = context;
		}


		//merge entries
		public List<Model_EntryItem> LoadEntryItem(Model_FilterEntry filter)
		{
			List<Model_EntryItem> result = [];
			if (filter.Share == ShareType.Own) result.AddRange(LoadUserEntryItems(filter));
			if (filter.Share == ShareType.Group) result.AddRange(LoadGroupEntryItems(filter));
			if (filter.Share == ShareType.Direcly) result.AddRange(LoadDirectlySharedEntryItems(filter));
			if (filter.Share == ShareType.Public) result.AddRange(LoadPublicEntryItems(filter));

			//Dont think tha it is neccacery because that should already be all unique items
			List<Model_EntryItem> entries = result.DistinctBy(s => s.ID).ToList();
			return entries;
		}

		//UserItems
		private List<Model_EntryItem> LoadUserEntryItems(Model_FilterEntry filter)
		{
			Guid userID = Guid.Empty;
			IEnumerable<Structure_Entry> list = _context.Structure_Entry
				.Where(s => s.UserID == userID)
				.Include(s => s.O_Folder)
				.Include(s => s.O_Template);

			if (!string.IsNullOrEmpty(filter.Name)) list = list.Where(s => s.Name.Contains(filter.Name));
			if (!string.IsNullOrEmpty(filter.TemplateName)) list = list.Where(s => s.O_Template.Name.Contains(filter.TemplateName));
			if (!string.IsNullOrEmpty(filter.Username)) list = list.Where(s => s.O_User.UsernameNormalized.Contains(filter.Username));

			List<Model_EntryItem> result = list.Select(s => new Model_EntryItem(s, ShareType.Own)).ToList();
			return result;
		}

		//directly shared
		private List<Model_EntryItem> LoadDirectlySharedEntryItems(Model_FilterEntry filter)
		{
			Guid userID = Guid.Empty;
			List<Model_EntryItem> result = [];

			IQueryable<Share_Item> sharedList = _context.Share_Item.Where(s => s.Group == false && s.ID == userID && s.ItemType == 1); //ItemType: 1 == entry

			foreach (Share_Item? item in sharedList)
			{
				IEnumerable<Structure_Entry> list = _context.Structure_Entry
				.Where(s => s.ID == item.ID)
				.Include(s => s.O_Folder)
				.Include(s => s.O_Template);

				if (!string.IsNullOrEmpty(filter.Name)) list = list.Where(s => s.Name.Contains(filter.Name));
				if (!string.IsNullOrEmpty(filter.TemplateName)) list = list.Where(s => s.O_Template.Name.Contains(filter.TemplateName));
				if (!string.IsNullOrEmpty(filter.Username)) list = list.Where(s => s.O_User.UsernameNormalized.Contains(filter.Username));

				//adding entries to List
				result.AddRange(list.Select(s => new Model_EntryItem(s, ShareType.Direcly)));
			}

			return result;
		}

		private List<Model_EntryItem> LoadPublicEntryItems(Model_FilterEntry filter)
		{
			List<Model_EntryItem> publicEntryItem = [];

			IEnumerable<Structure_Entry> list = _context.Structure_Entry
				//.Where(s => s.IsPublic)
				.Include(s => s.O_Folder)
				.Include(s => s.O_Template);

			if (!string.IsNullOrEmpty(filter.Name)) list = list.Where(s => s.Name.Contains(filter.Name));
			if (!string.IsNullOrEmpty(filter.TemplateName)) list = list.Where(s => s.O_Template.Name.Contains(filter.TemplateName));
			if (!string.IsNullOrEmpty(filter.Username)) list = list.Where(s => s.O_User.UsernameNormalized.Contains(filter.Username));

			List<Model_EntryItem> result = list.Select(s => new Model_EntryItem(s, ShareType.Public)).ToList();
			return result;
		}

		//group items
		private List<Model_EntryItem> LoadGroupEntryItems(Model_FilterEntry filter)
		{
			Guid userID = Guid.Empty;
			List<Model_EntryItem> result = [];

			//get all ShareGroups of User
			IQueryable<Share_Group> sharedList = _context.Share_GroupUser
				.Include(u => u.O_Group)
				.Where(u => u.UserID == userID)
				.Select(g => g.O_Group);


			foreach (Share_Group? groupItem in sharedList)
			{
				//All shared entries in the group 
				IQueryable<Share_Item> shareItem = _context.Share_Item.Where(s => s.Group == true && s.ID == groupItem.ID && s.ItemType == 1);

				foreach (Share_Item? item in shareItem)
				{
					IEnumerable<Structure_Entry> list = _context.Structure_Entry
					.Where(s => s.ID == item.ID)
					.Include(s => s.O_Folder)
					.Include(s => s.O_Template);

					if (!string.IsNullOrEmpty(filter.Name)) list = list.Where(s => s.Name.Contains(filter.Name));
					if (!string.IsNullOrEmpty(filter.TemplateName)) list = list.Where(s => s.O_Template.Name.Contains(filter.TemplateName));
					if (!string.IsNullOrEmpty(filter.Username)) list = list.Where(s => s.O_User.UsernameNormalized.Contains(filter.Username));

					//adding entries to List
					result.AddRange(list.Select(s => new Model_EntryItem(s, ShareType.Group)));
				}
			}

			return result;
		}
	}
}
