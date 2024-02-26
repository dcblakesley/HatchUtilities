namespace HatchUtilities;

public class WorkbookBuilder
{
    WorkBook? _workBook;
    static List<string>? _columnLetters;

    // Public Methods
    public void CreateWorkbook() => _workBook = new WorkBook(ExcelFileFormat.XLSX);
    public MemoryStream ToStream() => _workBook.ToStream();
    public void Save(string path) => _workBook.SaveAs(path);

    public void AddSheet(string name, List<List<object>> rowsOrColumns, bool autosize = false, bool transpose = false)
    {
        if (_workBook == null)
        {
            CreateWorkbook();
        }

        var workSheet = transpose
            ? AddSheetByColumns(name, rowsOrColumns)
            : AddSheetByRows(name, rowsOrColumns);

        if (autosize)
        {
            AutoSizeAllColumns(workSheet);
        }
    }

    public void AddSheet(string name, List<List<string>> rowsOrColumns, bool autosize = false, bool transpose = false)
        => AddSheet(name, StringsToObjects(rowsOrColumns), autosize, transpose);

    // Private Methods
    static void AutoSizeAllColumns(WorkSheet workSheet)
    {
        foreach (var column in workSheet.Columns)
        {
            column.AutoSizeColumn();
        }
    }

    static List<List<object>> StringsToObjects(List<List<string>> stringLists)
    {
        var objectLists = new List<List<object>>();
        foreach (var column in stringLists)
        {
            objectLists.Add(column.Select(x => (object)x).ToList());
        }

        return objectLists;
    }

    static List<string> GetColumnLetters()
    {
        if (_columnLetters != null)
        {
            return _columnLetters;
        }

        var lettersString = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
        var letters = lettersString.Split(",");
        var l2 = JsonSerializer.Deserialize<List<string>>(JsonSerializer.Serialize(letters));

        var columnLetters = new List<string>();

        // A - Z, 26
        var singles = lettersString.Split(",");

        // AA - ZZ, 702
        var doubles = new List<string>();
        foreach (var letter in letters)
        {
            doubles.AddRange(l2!.Select(l => letter + l));
        }

        // AAA - ZZZ, 18278
        var triples = new List<string>();
        foreach (var two in doubles)
        {
            triples.AddRange(letters.Select(l => two + l));
        }

        columnLetters.AddRange(singles);
        columnLetters.AddRange(doubles);
        columnLetters.AddRange(triples);

        _columnLetters = columnLetters;
        return columnLetters;
    }

    /// <summary> Send in a list of Rows </summary>
    WorkSheet AddSheetByRows(string name, List<List<object>> rows)
    {
        var columnLetters = GetColumnLetters();
        var workSheet = _workBook.CreateWorkSheet(name);

        var rowCounter = 0;

        foreach (var row in rows)
        {
            var columnCounter = 0;
            rowCounter++;
            foreach (var o in row)
            {
                workSheet[$"{columnLetters[columnCounter++]}{rowCounter}"].Value = o;
            }
        }

        return workSheet;
    }

    /// <summary> Send in a list of Columns </summary>
    WorkSheet AddSheetByColumns(string name, List<List<object>> columns)
    {
        var columnLetters = GetColumnLetters();
        var workSheet = _workBook.CreateWorkSheet(name);

        var columnCounter = 0;

        foreach (var column in columns)
        {
            var rowCounter = 1;
            foreach (var o in column)
            {
                workSheet[$"{columnLetters[columnCounter]}{rowCounter++}"].Value = o;
            }
            columnCounter++;
        }

        return workSheet;
    }
}