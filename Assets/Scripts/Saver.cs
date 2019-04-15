using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Threading;

public class Saver : MonoBehaviour
{
   public StreamWriter SW;
  public  StreamReader SR;
    float frequency = 10;
    static public string s = "A";

    int x = 0;
    public string S
    {
        get
        {
            return s;
        }
        set
        {
            s = value;
        }
    }

    private Saver()
    {
       
        FileCheck();
        // s = Data;
    }

    private static Saver instance;

    public void OpenStreamForWrite()
    {
        FileCheck();
        SW = new StreamWriter("Log1",true) ;
       
    }

    public void CloseStreanForWrite()
    {
        //SW.Flush();
        //SW.Close();
    }
    
    public void WriteInFile()
    {

        using (StreamWriter SR = new StreamWriter("Log1.txt)", true))
        {
            SR.WriteLine(s);
        }
        
            
    }

    public void OpenStreamForRead()
    {
        FileCheck();
        SR = File.OpenText("Log1.txt");
    }
    
    public void CloseStreamForRead()
    {
        SR.Dispose();
        SR.Close();
    }

    IEnumerator WaitForTime()
    {
        yield return new WaitForSeconds(frequency);
    }
    public string ReadFromFile()
    {
       
        WaitForTime();
        Debug.Log("Received");
        s = SR.ReadLine();
        if (s!=null)
        {
            return s;
        }
        else
        {
            return "Empty";
        }
        
    }

    public static Saver Get()
    {
        if (instance == null)
        {
            instance = new Saver();
        }
        return instance;
    }

  

    public void ClearFile()
    {
        File.Delete("Log1.txt");
    }

    //public string ReadFromData()
    //{

    //    string[] sr = File.ReadAllLines("Log.txt");
    //    x++;
    //    if (sr.Length < x)
    //    {
    //        return "Empty!!!";
    //    }
    //    return sr[x - 1];

    //    //using (StreamReader sr = File.OpenText("Log.txt"))
    //    //{

    //    //    // for (int i = 0; i <= x; i++)
    //    //    s = sr.ReadToEnd();
    //    //    Debug.Log(s);

    //    //    return (s);
    //    //}


    //}
    public void FileCheck()
    {
        if (!(File.Exists("Log1.txt")))
        {
            File.Create("Log1.txt");
        }
    }

    //    using (FileStream fs = File.Create(path)) 
    //{ 
    //AddText(fs, "This is some text");
    //AddText(fs, "This is some more text,");
    //AddText(fs, "\r\nand this is on a new line");
    //AddText(fs, "\r\n\r\nThe following is a subset of characters:\r\n"); 

    //for (int i=1;i< 120;i++) 
    //{ 

}




