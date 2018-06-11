using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{

    private static string TAG = "NetworkManager";
    //[Range(1.0f, 5.0f)]

    public Image validity;

    public Image cursorBox;

    private GameObject managerRef;

    //public Image trackedCenter;
   // private GameObject videoTexture = null;
   // private bool enlarged = false;
    private bool visible = false;
    private float[] lastTransform;
    private bool showCenter = true;
    private float showCenterX, showCenterY;

    private List<Alignment> alignments = new List<Alignment>();

    public struct Alignment
    {
        public int x, y;
        public float sx, sy;
        public float[] trans;

        public Alignment(int x, int y, float sx, float sy, float[] trans)
        {
            this.x = x;
            this.y = y;
            this.sx = sx;
            this.sy = sy;
            this.trans = new float[16];
            trans.CopyTo(this.trans, 0);
        }

        public override string ToString()
        {
            string str = "";
            str += "Pixel: " + x + ", " + y + "\n";
            str += "GroundTruth: " + sx + ", " + sy + "\n";
            str += "Location: " + trans[12] * 1000f + ", " + trans[13] * 1000f + ", " + trans[14] * 1000f + "\n";
            str += "Transformation: " + trans[0] + ", " + trans[1] + ", " + trans[2] + ", " + trans[3] + "\n";
            str += "Transformation: " + trans[4] + ", " + trans[5] + ", " + trans[6] + ", " + trans[7] + "\n";
            str += "Transformation: " + trans[8] + ", " + trans[9] + ", " + trans[10] + ", " + trans[11] + "\n";
            str += "Transformation: " + trans[12] + ", " + trans[13] + ", " + trans[14] + ", " + trans[15] + "\n";
            return str;
        }
    }

    // Use this for initialization
    void Start()
    {
        managerRef = GameObject.Find("Manager");
    }

    // Update is called once per frame
    void Update()
    {
        if (UDPCommunication.Messages.Count > 0)
        {
            string message;
            List<int> fields;
            while (UDPCommunication.Messages.Count > 0)
            {
                message = UDPCommunication.Messages.Dequeue();
                fields = message.Split(',').Select(Int32.Parse).ToList();
                if (fields[3] == 2)
                {
                    OnCursorLeftClick(fields[0], fields[1]);
                }
                else if (fields[3] == 3)
                {
                    OnCursorRightClick(fields[0], fields[1]);
                }
                else if (fields[3] == 4)
                {
                   // WriteFile();
                }
                if (UDPCommunication.Messages.Count == 0)
                {
                    cursorBox.rectTransform.transform.localPosition = new Vector3(fields[0], fields[1], 0);

                }
            }
        }

    }


    public void OnCursorLeftClick(int x, int y)
    {
        if (visible)
        {
            var a = new Alignment(x, y, showCenterX, showCenterY, lastTransform);
            alignments.Add(a);
        }
        validity.color = Color.green;
        managerRef.GetComponent<ReadWorkFlow>().next();

    }
    public void OnCursorRightClick(int x, int y)
    {
        if (alignments.Count > 0)
        {
            alignments.RemoveAt(alignments.Count - 1);
        }
        validity.color = Color.red;
        managerRef.GetComponent<ReadWorkFlow>().previous();

    }


    /*public void WriteFile()
    {
        FileInfo f = new FileInfo(Application.persistentDataPath + "/spaam.txt");
        StreamWriter w;
        if (!f.Exists)
        {
            w = f.CreateText();
        }
        else
        {
            f.Delete();
            w = f.CreateText();
        }
        foreach (var a in alignments)
        {
            w.WriteLine(a.ToString());
        }
        w.Close();
    }
    */
}
