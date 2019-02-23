using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Net;
using System.IO;
public class Video : MonoBehaviour
{


    public MeshRenderer frame;    //Mesh for displaying video

    private string sourceURL = "http://166.169.138.136:81/record/current.jpg?rand=297227";
    private Texture2D texture;
    private Stream stream;

    public void GetVideo()
    {
        texture = new Texture2D(2, 2);
        // create HTTP request
        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(sourceURL);
        //Optional (if authorization is Digest)
        req.Credentials = new NetworkCredential("username", "password");
        // get response
        WebResponse resp = req.GetResponse();
        // get response stream
        stream = resp.GetResponseStream();
        StartCoroutine(GetFrame());
    }

    IEnumerator GetFrame()
    {
        Byte[] JpegData = new Byte[65536];

        while (true)
        {
            int bytesToRead = FindLength(stream);
            print(bytesToRead);
            if (bytesToRead == -1)
            {
                print("End of stream");
                yield break;
            }

            int leftToRead = bytesToRead;

            while (leftToRead > 0)
            {
                leftToRead -= stream.Read(JpegData, bytesToRead - leftToRead, leftToRead);
                yield return null;
            }

            MemoryStream ms = new MemoryStream(JpegData, 0, bytesToRead, false, true);

            texture.LoadImage(ms.GetBuffer());
            frame.material.mainTexture = texture;
            stream.ReadByte(); // CR after bytes
            stream.ReadByte(); // LF after bytes
        }
    }

    int FindLength(Stream stream)
    {
        int b;
        string line = "";
        int result = -1;
        bool atEOL = false;

        while ((b = stream.ReadByte()) != -1)
        {
            if (b == 10) continue; // ignore LF char
            if (b == 13)
            { // CR
                if (atEOL)
                {  // two blank lines means end of header
                    stream.ReadByte(); // eat last LF
                    return result;
                }
                if (line.StartsWith("Content-Length:"))
                {
                    result = Convert.ToInt32(line.Substring("Content-Length:".Length).Trim());
                }
                else
                {
                    line = "";
                }
                atEOL = true;
            }
            else
            {
                atEOL = false;
                line += (char)b;
            }
        }
        return -1;

    }
}
