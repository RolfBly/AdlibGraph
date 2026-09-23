using Adlib.Database.IO;
using Adlib.Interfaces;
using Adlib.Setup;
using Adlib.Setup.Database;
using System;
using System.Collections.Generic;
using System.IO;

namespace DDigit.Graph
{
  public class DatabaseList : NodeList
  {
    internal void CreateEdges(FieldList fields, IndexList indexes, ScreenList screens)
    {
      foreach (var database in databases.Values)
      {
        var databaseInfo = database.Database;
        var databaseNode = FindDatabaseNode(databaseInfo);
        if (databaseNode != null)
        {
          foreach (var feedback in databaseInfo.FeedBackLinkList)
          {
            IAdlibDatabaseInfo feedbackDatabaseInfo;
            try
            {
              feedbackDatabaseInfo = feedback.DatabaseInfo;
            }
            catch (FileNotFoundException)
            {
              Console.WriteLine($"Waarschuwing: feedback database '{feedback.DatabaseName}' van '{databaseInfo.BaseName}' bestaat niet meer op schijf");
              continue;
            }
            if (databases.TryGetValue(DatabaseNode.DatabasePath(feedbackDatabaseInfo), out var feedbackNode))
            {
              AddEdge(databaseNode, AdlibEdgeType.RequiresFeedbackDatabase, feedbackNode);
            }
          }

          foreach (var fieldInfo in databaseInfo.FieldInfoCollection.Values)
          {
            var fieldNode = fields.FindFieldNode(databaseInfo, fieldInfo.Tag);
            if (fieldInfo.IsLinked)
            {
              LinkFieldNodeToDatabaseNode(databaseNode, fieldNode);
              var linkedFieldNode = fields.FindFieldNode(databaseInfo, fieldInfo.LinkRefTag);
              if (fieldNode != null && linkedFieldNode != null)
              {
                AddEdge(fieldNode, AdlibEdgeType.UsesLinkRef, linkedFieldNode);
                foreach (var indexNode in indexes.FindIndexNodes(databaseInfo, fieldInfo.LinkRefTag))
                {
                  AddEdge(linkedFieldNode, AdlibEdgeType.IndexedIn, indexNode);
                }
              }
              screens.LinkScreenToNode(databaseInfo, fieldNode, fieldInfo.LinkScreen, AdlibEdgeType.UsesLinkScreen);
              screens.LinkScreenToNode(databaseInfo, fieldNode, fieldInfo.ZoomScreen, AdlibEdgeType.UsesZoomScreen);
              screens.LinkScreenToNode(databaseInfo, fieldNode, fieldInfo.EditScreen, AdlibEdgeType.UsesEditScreen);
              screens.LinkScreenToNode(databaseInfo, fieldNode, ((FieldInfo)fieldInfo).SearchScreen, AdlibEdgeType.UsesSearchScreen);
              screens.LinkScreenToNode(databaseInfo, fieldNode, fieldInfo.DetailScreen, AdlibEdgeType.UsesDetailScreen);

              var linkedDatabaseNode = FindLinkedDatabaseNode(databaseInfo, fieldInfo);
              if (linkedDatabaseNode != null)
              {
                AddEdge(databaseNode, AdlibEdgeType.UsesDatabase, linkedDatabaseNode);
              }
            }

            if (!fieldInfo.IsLinkRef)
            {
              LinkFieldNodeToDatabaseNode(databaseNode, fieldNode);
              foreach (var indexNode in indexes.FindIndexNodes(databaseInfo, fieldInfo.Tag))
              {
                AddEdge(fieldNode, AdlibEdgeType.IndexedIn, indexNode);
              }
            }
          }
        }
      }
    }

    DatabaseNode FindLinkedDatabaseNode(DatabaseInfo databaseInfo, IFieldInfo fieldInfo)
    {
      IAdlibDatabaseInfo linkedDatabaseInfo = null;
      try
      {
        linkedDatabaseInfo = fieldInfo.LinkedDatabaseInfo;
      }
      catch (FileNotFoundException)
      {
        Console.WriteLine($"Waarschuwing: gelinkte database '{fieldInfo.LinkedDatabase}' van veld '{fieldInfo.Tag}' in '{databaseInfo.BaseName}' bestaat niet meer op schijf");
      }

      if (linkedDatabaseInfo != null && databases.TryGetValue(DatabaseNode.DatabasePath(linkedDatabaseInfo), out var node))
      {
        return node;
      }

      // LinkedDatabaseInfo can return null even when IsLinked is set and LinkedDatabase (the raw name) is populated,
      // e.g. for repeatable/multi-occurrence link fields and language-variant links. Fall back to a name lookup.
      if (!string.IsNullOrWhiteSpace(fieldInfo.LinkedDatabase) && databasesByName.TryGetValue(fieldInfo.LinkedDatabase, out var byName))
      {
        return byName;
      }

      return null;
    }

    void LinkFieldNodeToDatabaseNode(AdlibNode databaseNode, AdlibNode fieldNode)
    {
      if (databaseNode != null && fieldNode != null)
      {
        AddEdge(databaseNode, AdlibEdgeType.HasField, fieldNode);
      }
    }

    internal void LoadDatabases(DirectoryInfo directoryInfo, Storage storage, FieldList fields, IndexList indexes)
    {
      var fileInfo = directoryInfo.GetFiles("*.inf");
      foreach (var file in fileInfo)
      {
        if (file.FullName.EndsWith(".inf", StringComparison.CurrentCultureIgnoreCase))
        {
          var databaseInfo = new DatabaseInfo(new AdlibPath(file.FullName), storage);
          var databaseNode = new DatabaseNode(databaseInfo);
          databases[databaseNode.Path] = databaseNode;
          databasesByName[databaseInfo.BaseName] = databaseNode;

          foreach (var fieldInfo in databaseInfo.FieldInfoCollection.Values)
          {
            var fieldNode = new FieldNode(databaseInfo, fieldInfo);
            fields.Add(fieldNode.Path, fieldNode);
          }

          foreach (var indexInfo in databaseInfo.IndexList)
          {
            if (indexInfo.FirstIndexTag != "%0" && indexInfo.IndexName != "wordlist")
            {
              var indexNode = new IndexNode(databaseInfo, indexInfo);
              indexes.Add(indexNode.Path, indexNode);
            }
          }
        }
      }
    }

    DatabaseNode FindDatabaseNode(DatabaseInfo databaseInfo) => databases.TryGetValue(DatabaseNode.DatabasePath(databaseInfo), out DatabaseNode node) ? node : null;
    internal DatabaseNode this[string path] => databases[path];
    internal IEnumerable<DatabaseNode> Values => databases.Values;
    public int Count => databases.Count;

    readonly SortedDictionary<string, DatabaseNode> databases = new SortedDictionary<string, DatabaseNode>();
    readonly Dictionary<string, DatabaseNode> databasesByName = new Dictionary<string, DatabaseNode>(StringComparer.OrdinalIgnoreCase);

  }
}
