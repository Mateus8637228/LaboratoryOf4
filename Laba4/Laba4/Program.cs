using System;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Text;

namespace TextFileApplication
{
  [Serializable]
  public class TextDocument
  {
    public string fileName;
    public string fileContent;
    public DateTime lastModifiedTime;
    public string filePath;

    public TextDocument()
    {
      lastModifiedTime = DateTime.Now;
    }

    public TextDocument(string path)
    {
      filePath = path;
      fileName = Path.GetFileName(path);
      fileContent = File.ReadAllText(path);
      lastModifiedTime = File.GetLastWriteTime(path);
    }

    public void SaveToBinaryFormat(string targetFilePath)
    {
      FileStream fileStream = new FileStream(targetFilePath, FileMode.Create);
      BinaryFormatter binaryFormatter = new BinaryFormatter();

      binaryFormatter.Serialize(fileStream, this);

      fileStream.Close();
    }

    public static TextDocument LoadFromBinaryFormat(string sourceFilePath)
    {
      FileStream fileStream = new FileStream(sourceFilePath, FileMode.Open);
      BinaryFormatter binaryFormatter = new BinaryFormatter();

      TextDocument loadedDocument = (TextDocument)binaryFormatter.Deserialize(fileStream);

      fileStream.Close();

      return loadedDocument;
    }

    public void SaveToXmlFormat(string targetFilePath)
    {
      StreamWriter streamWriter = new StreamWriter(targetFilePath);
      XmlSerializer xmlSerializer = new XmlSerializer(typeof(TextDocument));

      xmlSerializer.Serialize(streamWriter, this);

      streamWriter.Close();
    }

    public static TextDocument LoadFromXmlFormat(string sourceFilePath)
    {
      StreamReader streamReader = new StreamReader(sourceFilePath);
      XmlSerializer xmlSerializer = new XmlSerializer(typeof(TextDocument));

      TextDocument loadedDocument = (TextDocument)xmlSerializer.Deserialize(streamReader);

      streamReader.Close();

      return loadedDocument;
    }

    public void SaveToOriginalFile()
    {
      File.WriteAllText(filePath, fileContent);
      lastModifiedTime = DateTime.Now;
    }
  }

  public class DocumentSnapshot
  {
    public string snapshotContent;
    public DateTime snapshotCreationTime;

    public DocumentSnapshot(string content)
    {
      snapshotContent = content;
      snapshotCreationTime = DateTime.Now;
    }
  }

  public class DocumentEditor
  {
    private TextDocument currentDocument;
    private Stack<DocumentSnapshot> changeHistory;

    public DocumentEditor(TextDocument document)
    {
      currentDocument = document;
      changeHistory = new Stack<DocumentSnapshot>();

      SaveCurrentStateToHistory();
    }

    private void SaveCurrentStateToHistory()
    {
      DocumentSnapshot newSnapshot = new DocumentSnapshot(currentDocument.fileContent);

      changeHistory.Push(newSnapshot);
    }

    public void UpdateDocumentContent(string newContent)
    {
      currentDocument.fileContent = newContent;

      SaveCurrentStateToHistory();
    }

    public bool RestorePreviousVersion()
    {
      if (changeHistory.Count > 1)
      {
        changeHistory.Pop();

        DocumentSnapshot previousSnapshot = changeHistory.Peek();
        currentDocument.fileContent = previousSnapshot.snapshotContent;

        return true;
      }

      return false;
    }

    public void ShowEditHistory()
    {
      Console.WriteLine("\nEdit history:");

      int historyItemIndex = 1;

      foreach (DocumentSnapshot snapshot in changeHistory)
      {
        Console.WriteLine(historyItemIndex + ". [" + snapshot.snapshotCreationTime + "] - " + snapshot.snapshotContent.Length + " characters");

        historyItemIndex++;
      }
    }

    public void DisplayCurrentContent()
    {
      Console.WriteLine("\nCurrent content of file " + currentDocument.fileName + ":");
      Console.WriteLine("--------------------------------------------------");
      Console.WriteLine(currentDocument.fileContent);
      Console.WriteLine("--------------------------------------------------");
    }

    public void SaveCurrentDocument()
    {
      currentDocument.SaveToOriginalFile();

      Console.WriteLine("File saved successfully.");
    }
  }

  public class FileContentSearcher
  {
    public List<string> FindFilesWithKeywords(string searchDirectory, List<string> searchKeywords, bool includeSubdirectories)
    {
      List<string> foundFiles = new List<string>();

      SearchOption directorySearchOption;

      if (includeSubdirectories)
      {
        directorySearchOption = SearchOption.AllDirectories;
      }
      else
      {
        directorySearchOption = SearchOption.TopDirectoryOnly;
      }

      string[] allTextFiles = Directory.GetFiles(searchDirectory, "*.txt", directorySearchOption);

      for (int fileIndex = 0; fileIndex < allTextFiles.Length; fileIndex++)
      {
        string currentFilePath = allTextFiles[fileIndex];

        try
        {
          string fileTextContent = File.ReadAllText(currentFilePath);

          for (int keywordIndex = 0; keywordIndex < searchKeywords.Count; keywordIndex++)
          {
            string currentKeyword = searchKeywords[keywordIndex];

            if (fileTextContent.IndexOf(currentKeyword, StringComparison.OrdinalIgnoreCase) >= 0)
            {
              if (!foundFiles.Contains(currentFilePath))
              {
                foundFiles.Add(currentFilePath);
              }
            }
          }
        }
        catch (Exception error)
        {
          Console.WriteLine("Error reading file " + currentFilePath + ": " + error.Message);
        }
      }

      return foundFiles;
    }
  }

  public class FileIndexerSystem
  {
    private Dictionary<string, List<string>> searchIndex;

    public FileIndexerSystem()
    {
      searchIndex = new Dictionary<string, List<string>>();
    }

    public void BuildIndexFromDirectory(string targetDirectory, List<string> keywordsForIndex, bool recursiveScan)
    {
      searchIndex.Clear();

      SearchOption scanOption;

      if (recursiveScan)
      {
        scanOption = SearchOption.AllDirectories;
      }
      else
      {
        scanOption = SearchOption.TopDirectoryOnly;
      }

      string[] textFilesInDirectory = Directory.GetFiles(targetDirectory, "*.txt", scanOption);

      for (int currentFileIndex = 0; currentFileIndex < textFilesInDirectory.Length; currentFileIndex++)
      {
        string currentFilePath = textFilesInDirectory[currentFileIndex];

        try
        {
          string fileContents = File.ReadAllText(currentFilePath).ToLower();

          for (int currentKeywordIndex = 0; currentKeywordIndex < keywordsForIndex.Count; currentKeywordIndex++)
          {
            string currentKeyword = keywordsForIndex[currentKeywordIndex];

            if (fileContents.Contains(currentKeyword.ToLower()))
            {
              if (!searchIndex.ContainsKey(currentKeyword))
              {
                searchIndex[currentKeyword] = new List<string>();
              }

              if (!searchIndex[currentKeyword].Contains(currentFilePath))
              {
                searchIndex[currentKeyword].Add(currentFilePath);
              }
            }
          }
        }
        catch (Exception error)
        {
          Console.WriteLine("Error indexing file " + currentFilePath + ": " + error.Message);
        }
      }
    }

    public List<string> SearchByKeyword(string keywordToFind)
    {
      string normalizedKeyword = keywordToFind.ToLower();

      if (searchIndex.ContainsKey(normalizedKeyword))
      {
        return searchIndex[normalizedKeyword];
      }

      return new List<string>();
    }

    public void DisplayCurrentIndex()
    {
      Console.WriteLine("\nCurrent file index by keywords:");

      foreach (KeyValuePair<string, List<string>> indexEntry in searchIndex)
      {
        Console.WriteLine("\nKeyword: " + indexEntry.Key);

        for (int fileListIndex = 0; fileListIndex < indexEntry.Value.Count; ++fileListIndex)
        {
          Console.WriteLine("  - " + indexEntry.Value[fileListIndex]);
        }
      }
    }
  }

  class Program
  {
    static void Main(string[] arguments)
    {
      Console.WriteLine("=== Text Editor with Indexing System ===\n");

      bool continueRunning = true;

      while (continueRunning)
      {
        Console.WriteLine("\nSelect an option:");
        Console.WriteLine("1. Use document editor");
        Console.WriteLine("2. Search files by keywords");
        Console.WriteLine("3. Index directory contents");
        Console.WriteLine("4. Exit application");

        string userChoice = Console.ReadLine();

        if (userChoice == "1")
        {
          RunDocumentEditor();
        }
        else if (userChoice == "2")
        {
          RunKeywordSearch();
        }
        else if (userChoice == "3")
        {
          RunDirectoryIndexer();
        }
        else if (userChoice == "4")
        {
          continueRunning = false;
        }
        else
        {
          Console.WriteLine("Invalid selection. Please try again.");
        }
      }
    }

    static void RunDocumentEditor()
    {
      Console.Write("Enter path to text file: ");

      string filePath = Console.ReadLine();

      if (!File.Exists(filePath))
      {
        Console.WriteLine("File does not exist. Create new file? (y/n)");

        string createNewResponse = Console.ReadLine();

        if (createNewResponse.ToLower() == "y")
        {
          File.WriteAllText(filePath, "");
        }
        else
        {
          return;
        }
      }

      TextDocument currentDocument = new TextDocument(filePath);
      DocumentEditor editor = new DocumentEditor(currentDocument);

      bool continueEditing = true;

      while (continueEditing)
      {
        Console.WriteLine("\nEditor menu:");
        Console.WriteLine("1. View document content");
        Console.WriteLine("2. Edit document content");
        Console.WriteLine("3. Undo last change");
        Console.WriteLine("4. View edit history");
        Console.WriteLine("5. Save document");
        Console.WriteLine("6. Save as binary format");
        Console.WriteLine("7. Save as XML format");
        Console.WriteLine("8. Return to main menu");

        string editorChoice = Console.ReadLine();

        if (editorChoice == "1")
        {
          editor.DisplayCurrentContent();
        }
        else if (editorChoice == "2")
        {
          Console.WriteLine("Enter new text (empty line to finish):");

          StringBuilder newContentBuilder = new StringBuilder();
          string currentLine = Console.ReadLine();

          while (!string.IsNullOrEmpty(currentLine))
          {
            newContentBuilder.AppendLine(currentLine);
            currentLine = Console.ReadLine();
          }

          editor.UpdateDocumentContent(newContentBuilder.ToString());

          Console.WriteLine("Document content updated.");
        }
        else if (editorChoice == "3")
        {
          bool undoPerformed = editor.RestorePreviousVersion();

          if (undoPerformed)
          {
            Console.WriteLine("Last change undone.");
          }
          else
          {
            Console.WriteLine("Cannot undo - no previous versions available.");
          }
        }
        else if (editorChoice == "4")
        {
          editor.ShowEditHistory();
        }
        else if (editorChoice == "5")
        {
          editor.SaveCurrentDocument();
        }
        else if (editorChoice == "6")
        {
          string binaryFilePath = Path.ChangeExtension(filePath, ".bin");

          currentDocument.SaveToBinaryFormat(binaryFilePath);

          Console.WriteLine("Document saved as binary to " + binaryFilePath);
        }
        else if (editorChoice == "7")
        {
          string xmlFilePath = Path.ChangeExtension(filePath, ".xml");

          currentDocument.SaveToXmlFormat(xmlFilePath);

          Console.WriteLine("Document saved as XML to " + xmlFilePath);
        }
        else if (editorChoice == "8")
        {
          continueEditing = false;
        }
        else
        {
          Console.WriteLine("Invalid selection. Please try again.");
        }
      }
    }

    static void RunKeywordSearch()
    {
      Console.Write("Enter directory path for search: ");

      string searchDirectory = Console.ReadLine();

      if (!Directory.Exists(searchDirectory))
      {
        Console.WriteLine("Directory does not exist.");

        return;
      }

      Console.Write("Enter keywords separated by commas: ");

      string keywordsInput = Console.ReadLine();
      string[] keywordsArray = keywordsInput.Split(',');
      List<string> keywordsList = new List<string>();

      for (int keywordIndex = 0; keywordIndex < keywordsArray.Length; keywordIndex++)
      {
        keywordsList.Add(keywordsArray[keywordIndex].Trim());
      }

      FileContentSearcher searcher = new FileContentSearcher();
      List<string> searchResults = searcher.FindFilesWithKeywords(searchDirectory, keywordsList, true);

      Console.WriteLine("\nFiles found: " + searchResults.Count);

      for (int resultIndex = 0; resultIndex < searchResults.Count; ++resultIndex)
      {
        Console.WriteLine("- " + searchResults[resultIndex]);
      }
    }

    static void RunDirectoryIndexer()
    {
      Console.Write("Enter directory path for indexing: ");

      string targetDirectory = Console.ReadLine();

      if (!Directory.Exists(targetDirectory))
      {
        Console.WriteLine("Directory does not exist.");

        return;
      }

      Console.Write("Enter keywords for indexing separated by commas: ");

      string indexKeywordsInput = Console.ReadLine();
      string[] indexKeywordsArray = indexKeywordsInput.Split(',');
      List<string> indexKeywordsList = new List<string>();

      for (int keywordIndex = 0; keywordIndex < indexKeywordsArray.Length; keywordIndex++)
      {
        indexKeywordsList.Add(indexKeywordsArray[keywordIndex].Trim());
      }

      FileIndexerSystem indexer = new FileIndexerSystem();

      indexer.BuildIndexFromDirectory(targetDirectory, indexKeywordsList, true);

      indexer.DisplayCurrentIndex();

      bool continueIndexing = true;

      while (continueIndexing)
      {
        Console.WriteLine("\nIndexer menu:");
        Console.WriteLine("1. Search by keyword");
        Console.WriteLine("2. Display full index");
        Console.WriteLine("3. Return to main menu");

        string indexerChoice = Console.ReadLine();

        if (indexerChoice == "1")
        {
          Console.Write("Enter keyword to search: ");

          string searchKeyword = Console.ReadLine();

          List<string> indexedFiles = indexer.SearchByKeyword(searchKeyword);

          Console.WriteLine("\nFiles found for keyword '" + searchKeyword + "': " + indexedFiles.Count);

          for (int fileIndex = 0; fileIndex < indexedFiles.Count; ++fileIndex)
          {
            Console.WriteLine("- " + indexedFiles[fileIndex]);
          }
        }
        else if (indexerChoice == "2")
        {
          indexer.DisplayCurrentIndex();
        }
        else if (indexerChoice == "3")
        {
          continueIndexing = false;
        }
        else
        {
          Console.WriteLine("Invalid selection. Please try again.");
        }
      }
    }
  }
}