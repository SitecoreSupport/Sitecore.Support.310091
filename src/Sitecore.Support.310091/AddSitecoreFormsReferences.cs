namespace Sitecore.Support.Publishing.Pipelines.GetItemReferences
{
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Links;
  using Sitecore.Publishing;
  using Sitecore.Publishing.Pipelines.GetItemReferences;
  using Sitecore.Publishing.Pipelines.PublishItem;
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class AddSitecoreFormsReferences : GetItemReferencesProcessor
  {
    protected override List<Item> GetItemReferences(PublishItemContext context)
    {
      Assert.ArgumentNotNull(context, "context");

      List<Item> list = new List<Item>();

      if (context.PublishOptions.Mode != PublishMode.SingleItem)
      {
        return list;
      }

      if (context.Action == PublishAction.PublishVersion)
      {
        if (context.VersionToPublish == null)
        {
          return list;
        }

        try
        {
          list.AddRange(GetSitecoreFormsRelatedItems(context));
        }
        catch (Exception ex)
        {
          Log.Warn("[AddSitecoreFormsReferences] Some 'Form' item descendants may not have be published" + Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace, this);
        }
      }

      return list;
    }

    private IEnumerable<Item> GetSitecoreFormsRelatedItems(PublishItemContext context)
    {
      var formTemplateId = ID.Parse("{6ABEE1F2-4AB4-47F0-AD8B-BDB36F37F64C}"); // "/sitecore/templates/System/Forms/Form"  template ID

      Item item = null;
      var relatedItems = new List<Item>();

      var itemId = context.ItemId;
      var database = context.PublishContext.PublishOptions.SourceDatabase;
      var languages = context.PublishContext.Languages;

      foreach (var language in languages)
      {
        item = database.GetItem(itemId, language);

        var links = GetRenderingReferences(item);

        foreach (var link in links)
        {
          var relatedItem = database.GetItem(link.TargetItemID, language);

          if (relatedItem != null && (relatedItem.TemplateID == formTemplateId))
          {
            var descendants = relatedItem.Axes.GetDescendants();

            relatedItems.AddRange(descendants);
          }
        }
      }

      return relatedItems;
    }

    private ItemLink[] GetRenderingReferences(Item item)
    {
      Assert.ArgumentNotNull(item, "item");

      var sourceDatabaseName = item.Database.Name;

      var renderingFieldId = ID.Parse("F1A1FE9E-A60C-4DDB-A3A0-BB5B29FE732E");
      var finalRenderingsFieldId = ID.Parse("04BF00DB-F5FB-41F7-8AB7-22408372A981");

      LinkDatabase linkDatabase = Globals.LinkDatabase;
      ItemLink[] references = linkDatabase.GetReferences(item).Where(link => link.SourceDatabaseName == sourceDatabaseName && (link.SourceFieldID == renderingFieldId || link.SourceFieldID == finalRenderingsFieldId)).ToArray();

      return references;
    }
  }
}