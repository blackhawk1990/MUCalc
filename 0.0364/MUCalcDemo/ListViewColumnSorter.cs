using System.Collections;
using System.Windows.Forms;


public class ListViewColumnSorter : IComparer
{
    private int ColumnToSort; //kolumna do posortowania

    private SortOrder OrderOfSort; //kolejnosc sortowania

    private CaseInsensitiveComparer ObjectCompare; //obiekt porownywania


    public ListViewColumnSorter()
    {
        //inicjalizacja
        ColumnToSort = 0;

        OrderOfSort = SortOrder.None;

        ObjectCompare = new CaseInsensitiveComparer();
    }

    //ta metoda porownuje dwa obiekty, zwracajac 0, gdy sa takie same, liczbe dodatnia - x > y, liczbe ujemna - x < y
    public int Compare(object x, object y)
    {
        int compareResult;
        ListViewItem listviewX, listviewY;

        //porownywane obiekty
        listviewX = (ListViewItem)x;
        listviewY = (ListViewItem)y;

        //porownanie
        compareResult = ObjectCompare.Compare(listviewX.SubItems[ColumnToSort].Text, listviewY.SubItems[ColumnToSort].Text);

        //w zaleznosci od ustawionego sortowania zwracamy normalny wynik sortowania dla ust. rosnacego lub odwrotny dla ust. malejacego
        if (OrderOfSort == SortOrder.Ascending)
        {
            return compareResult;
        }
        else if (OrderOfSort == SortOrder.Descending)
        {
            return (-compareResult);
        }
        else
        {
            // Return '0' to indicate they are equal
            return 0;
        }
    }

    public int SortColumn
    {
        set
        {
            ColumnToSort = value;
        }
        get
        {
            return ColumnToSort;
        }
    }

    public SortOrder Order
    {
        set
        {
            OrderOfSort = value;
        }
        get
        {
            return OrderOfSort;
        }
    }

}