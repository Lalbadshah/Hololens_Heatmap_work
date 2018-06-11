using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

#if !UNITY_EDITOR && UNITY_WSA
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Text;
#endif


public class ReadWorkFlow: MonoBehaviour {

    public Text DisplayTextFab;
    public Image DisplayImgFab;
    public GameObject PointFab;
	public GameObject Heatmap_canvas;

	private string JsonPath;
    private string jsonResourcesPath;
    private string jsonString;
    private int currentStep = 0;
    public List<Step> steps;
    private WorkFlow flow;

    private GameObject Display;
    private GameObject Marker;
    private Text stepName;
    private Text stepNumber;
    private List<DisplayAnchoredText> displayText;
    private List<DisplayAnchoredImage> displayImage;

    private List<FeatureAnchoredObject> featObjects;

    public float timeDelayForMultipleObjects = 5.0f;

    private bool hideOverlays = false;

    public List<string> jsonFiles;

	

#if !UNITY_EDITOR && UNITY_METRO
    private static Stream OpenFileForRead( string folderName, string fileName ) {
        Stream stream = null;
        bool taskFinish = false;
#if !UNITY_EDITOR && UNITY_METRO
        Task task = new Task(
            async () => {
                try {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync( folderName );
                    var item = await folder.TryGetItemAsync( fileName );
                    if( item != null ) {
                        StorageFile file = await folder.GetFileAsync( fileName );
                        if( file != null ) {
                            stream = await file.OpenStreamForReadAsync();
                        }
                    }
                }
                catch( Exception ) { }
                finally { taskFinish = true; }

            } );
        task.Start();
        while( !taskFinish ) {
            task.Wait();
        }
#endif
        return stream;
    }
#endif

	// Use this for initialization
	void Start() {
        Display = GameObject.Find( "DisplayAnchored" );
      //  Display = GameObject.Find("/Canvas/DisplayAnchored");
        Marker = GameObject.Find( "Marker1" );
        stepName = GameObject.Find( "StepName" ).GetComponent<Text>();
        stepNumber = GameObject.Find( "StepNumber" ).GetComponent<Text>();

        if( jsonFiles.Count > 0 ) {
            if( string.IsNullOrEmpty( jsonFiles[currentTask] ) ) {
                Debug.Log( "Filename is empty!" );
                return;
            }
#if !UNITY_EDITOR && UNITY_WSA
            //HoloLens code
            JsonPath = ApplicationData.Current.RoamingFolder.Path + "\\" + jsonFiles[0];
#else
            JsonPath = Application.streamingAssetsPath + "/" + jsonFiles[0];
#endif
            // Code to deal with android compatibility
            if( JsonPath.Contains( "://" ) ) { // When in android
                WWW www = new WWW( JsonPath );
                //yield return www;
                while( !www.isDone ) { }

                jsonString = www.text;
                Debug.Log( "You entered the android section: " + JsonPath );
            }
            else {// When not in android
#if !UNITY_EDITOR && UNITY_WSA
                //HoloLens code
                Debug.Log( "You entered the Hololens section: " + JsonPath );
                try {
                    using (Stream stream = OpenFileForRead(ApplicationData.Current.RoamingFolder.Path, jsonFiles[0])) {
                        byte[] data = new byte[stream.Length];
                        stream.Read(data, 0, data.Length);
                        jsonString = Encoding.ASCII.GetString(data);
                    }
                }
                catch (Exception e) {
                    Debug.Log(e);
                }
#else
                jsonString = File.ReadAllText( JsonPath );
                Debug.Log( "You entered the non-android section: " + JsonPath );
#endif
            }
            initialize( jsonString );
        }
    }

    void initialize( string jsonString ) {
        flow = JsonUtility.FromJson<WorkFlow>( jsonString );

        steps = flow.JuxtopiaTask.Steps;
        jsonResourcesPath = flow.JuxtopiaTask.ResourcesPath;
        DisplayStep( steps[currentStep] );
    }

    double timer = 0.0;
    int currentImagetoBeDisplayed;
    int currentTexttoBeDisplayed;
    private List<Image> imageList;
    private List<Text> textList;

    // Update is called once per frame
    void Update() {
        if( timeDelayForMultipleObjects != 0.0f ) {
            timer += Time.deltaTime;
            if( timer > timeDelayForMultipleObjects ) {
                timer = 0.0f;
                currentImagetoBeDisplayed++;
                currentTexttoBeDisplayed++;
                if( currentImagetoBeDisplayed >= imageList.Count ) {
                    currentImagetoBeDisplayed = 0;
                }
                if( currentTexttoBeDisplayed >= textList.Count ) {
                    currentTexttoBeDisplayed = 0;
                }
                foreach( Image img in imageList ) {
                    img.enabled = false;
                }
                foreach( Text txt in textList ) {
                    txt.enabled = false;
                }
                if( imageList.Count > 0 ) {
                    imageList[currentImagetoBeDisplayed].enabled = !hideOverlays;
                }
                if( textList.Count > 0) {
                    textList[currentTexttoBeDisplayed].enabled = !hideOverlays;
                }
            }
        }
        else {
            foreach( Image img in imageList ) {
                img.enabled = !hideOverlays;
            }
            foreach( Text txt in textList ) {
                txt.enabled = !hideOverlays;
            }
        }
}

    void DisplayStep( Step step ) {
        // Get rid of leftovers from older steps
        foreach( Transform child in Display.transform ) {
            GameObject.Destroy( child.gameObject );
        }

        // Change step name & number
        stepName.text = step.Name;
        stepNumber.text = "step " + currentStep.ToString();

        displayText = step.DisplayAnchoredText;
        displayImage = step.DisplayAnchoredImage;

        featObjects = step.FeatureAnchoredObjects;

        timer = timeDelayForMultipleObjects;

        textList = new List<Text>();

        foreach( DisplayAnchoredText entry in displayText ) {
            double[] position = entry.Position.ToArray();
            float scale = entry.Scale;

            Vector3 pos = new Vector3( (float)position[0], (float)position[1], (float)position[2] );
            Text newTextInstance = MonoBehaviour.Instantiate( DisplayTextFab ) as Text;
            newTextInstance.transform.SetParent( Display.transform );
            newTextInstance.transform.localPosition = pos;
            newTextInstance.transform.localRotation = Quaternion.identity;
            if( scale > 0 ) {
                newTextInstance.transform.localScale = Vector3.Scale( newTextInstance.transform.localScale, new Vector3( scale, scale, 1 ) );
            }
            
            textList.Add( newTextInstance );
            newTextInstance.text = entry.Text;
            newTextInstance.gameObject.SetActive( true );
        }
        currentTexttoBeDisplayed = textList.Count - 1;
        imageList = new List<Image>();

        foreach( DisplayAnchoredImage myImg in displayImage ) {
            if( myImg.Image_Path == "" ) {
                continue;
            }
            double[] position = myImg.Position.ToArray();
            float scale = myImg.Scale;

            Vector3 pos = new Vector3( (float)position[0], (float)position[1], (float)position[2] );
            //Create new UI image object for use
            Image newImgInstance = MonoBehaviour.Instantiate( DisplayImgFab ) as Image;
            newImgInstance.transform.SetParent( Display.transform );
            newImgInstance.transform.localPosition = pos;
            newImgInstance.transform.localRotation = Quaternion.identity;


            imageList.Add( newImgInstance );

            var tex = new Texture2D( 512, 512 );

            string filePath = Application.streamingAssetsPath + "/Resources/" + jsonResourcesPath + myImg.Image_Path;

            // Code to deal with android compatibility
            if( filePath.Contains( "://" ) ) { // When in android
                WWW www = new WWW( filePath );
                //yield return www;
                while( !www.isDone ) { }

                tex.LoadImage( www.bytes );
            }
            else {                       // When not in android
                tex.LoadImage( File.ReadAllBytes( filePath ) );
            }


            if( tex == null )
                Debug.Log( myImg.Image_Path + " not loaded properly" );
            newImgInstance.sprite = Sprite.Create( ( tex as Texture2D ), new Rect( 0.0f, 0.0f, tex.width, tex.height ), new Vector2( 0, 0 ), 100 );
            int width = newImgInstance.sprite.texture.width;
            int height = newImgInstance.sprite.texture.height;
            newImgInstance.transform.localScale = Vector3.Scale( newImgInstance.transform.localScale, new Vector3( (float)width / height, 1, 1 ) );
            if( scale > 0 ) {
                newImgInstance.transform.localScale = Vector3.Scale( newImgInstance.transform.localScale, new Vector3( scale, scale, 1 ) );
            }
            newImgInstance.gameObject.SetActive( true );




        }
        currentImagetoBeDisplayed = imageList.Count - 1;

        //Clears pre-existing Marker objects before creating new ones
        foreach( Transform child in Marker.transform ) {
            GameObject.Destroy( child.gameObject );
        }

        foreach( FeatureAnchoredObject feat in featObjects ) {
            double[] position = feat.Position.ToArray();
            Vector3 pos = new Vector3( (float)position[0], (float)position[1], (float)position[2] );
            if( feat.Image_Path.ToLower().Equals( "point" ) ) {
                GameObject featureInstance = MonoBehaviour.Instantiate( PointFab );
                featureInstance.transform.SetParent( Marker.transform );
                featureInstance.transform.localPosition = pos;
                featureInstance.transform.localScale = new Vector3(1, 1, 1);
                //featureInstance.layer = LayerMask.NameToLayer( "AR foreground" );
            }

        }



    }

	public void next() {
		currentStep = (currentStep + 1) % steps.Count;
		DisplayStep (steps [currentStep]);
        Debug.Log( "next" );
	}

	public void previous() {
		currentStep = (currentStep - 1) % steps.Count;
		if (currentStep < 0)
			currentStep = steps.Count + currentStep;
		DisplayStep (steps [currentStep]);
        Debug.Log( "previous" );
    }

    private int currentTask = 0;

    public void switchTask() {
        Debug.Log( "switchTask" );
        if( jsonFiles.Count > 0 ) {
            currentTask = ( currentTask + 1 ) % jsonFiles.Count;
            currentStep = 0;
            if( string.IsNullOrEmpty( jsonFiles[currentTask] ) ) {
                Debug.Log("Filename is empty!");
                return;
            }
#if !UNITY_EDITOR && UNITY_WSA
            //HoloLens code
            JsonPath = ApplicationData.Current.RoamingFolder.Path + "\\" + jsonFiles[currentTask];
#else
            JsonPath = Application.streamingAssetsPath + "/" + jsonFiles[currentTask];
#endif
            // Code to deal with android compatibility
            if( JsonPath.Contains( "://" ) ) { // When in android
                WWW www = new WWW( JsonPath );
                //yield return www;
                while( !www.isDone ) { }

                jsonString = www.text;
                Debug.Log( "You entered the android section: " + JsonPath );
            }
            else {                        // When not in android
#if !UNITY_EDITOR && UNITY_WSA
                //HoloLens code
                Debug.Log( "You entered the Hololens section: " + JsonPath );
                try {
                    using (Stream stream = OpenFileForRead(ApplicationData.Current.RoamingFolder.Path, jsonFiles[currentTask])) {
                        byte[] data = new byte[stream.Length];
                        stream.Read(data, 0, data.Length);
                        jsonString = Encoding.ASCII.GetString(data);
                    }
                }
                catch (Exception e) {
                    Debug.Log(e);
                }
#else
                jsonString = File.ReadAllText( JsonPath );
                Debug.Log( "You entered the non-android section: " + JsonPath );
#endif
            }

            initialize( jsonString );
        }
    }

    public void hide() {
        if( !hideOverlays ) {
            hideOverlays = true;
            foreach( Image img in imageList ) {
                img.enabled = false;
            }
            foreach( Text txt in textList ) {
                txt.enabled = false;
            }
            foreach( Transform child in Marker.transform ) {
                child.gameObject.SetActive( false );
            }
        }
    }

	public void showHM(){
		Heatmap_canvas.SetActive(true);
	}
	public void HideHM()
	{
		Heatmap_canvas.SetActive(false);
	}

	public void show() {
        if( hideOverlays ) {
            hideOverlays = false;
            timer = timeDelayForMultipleObjects;
            currentImagetoBeDisplayed = imageList.Count - 1;
            currentTexttoBeDisplayed = textList.Count - 1;
            foreach( Image img in imageList ) {
                img.enabled = false;
            }
            foreach( Text txt in textList ) {
                txt.enabled = false;

            }
            if( imageList.Count > 0 ) {
                imageList[0].enabled = true;
            }
            if( textList.Count > 0 ) {
                textList[0].enabled = true;
            }
            foreach( Transform child in Marker.transform ) {
                child.gameObject.SetActive( true );
            }
        }
    }


    public void reload() {
        Debug.Log( "Reload" );
        if( jsonFiles.Count > 0 ) {
            if( string.IsNullOrEmpty( jsonFiles[currentTask] ) ) {
                Debug.Log( "Filename is empty!" );
                return;
            }
#if !UNITY_EDITOR && UNITY_WSA
            //HoloLens code
            JsonPath = ApplicationData.Current.RoamingFolder.Path + "\\" + jsonFiles[currentTask];
#else
            JsonPath = Application.streamingAssetsPath + "/" + jsonFiles[currentTask];
#endif
            // Code to deal with android compatibility
            if( JsonPath.Contains( "://" ) ) { // When in android
                WWW www = new WWW( JsonPath );
                //yield return www;
                while( !www.isDone ) { }

                jsonString = www.text;
                Debug.Log( "You entered the android section: " + JsonPath );
            }
            else {                        // When not in android
#if !UNITY_EDITOR && UNITY_WSA
                //HoloLens code
                Debug.Log( "You entered the Hololens section: " + JsonPath );
                try {
                    using( Stream stream = OpenFileForRead( ApplicationData.Current.RoamingFolder.Path, jsonFiles[currentTask] ) ) {
                        byte[] data = new byte[stream.Length];
                        stream.Read( data, 0, data.Length );
                        jsonString = Encoding.ASCII.GetString( data );
                    }
                }
                catch( Exception e ) {
                    Debug.Log( e );
                }
#else
                jsonString = File.ReadAllText( JsonPath );
                Debug.Log( "You entered the non-android section: " + JsonPath );
#endif
            }

            initialize( jsonString );
        }
    }

    public void reset() {
        Debug.Log( "Reset" );
        if( jsonFiles.Count > 0 ) {
            currentStep = 0;
            if( string.IsNullOrEmpty( jsonFiles[currentTask] ) ) {
                Debug.Log( "Filename is empty!" );
                return;
            }
#if !UNITY_EDITOR && UNITY_WSA
            //HoloLens code
            JsonPath = ApplicationData.Current.RoamingFolder.Path + "\\" + jsonFiles[currentTask];
#else
            JsonPath = Application.streamingAssetsPath + "/" + jsonFiles[currentTask];
#endif
            // Code to deal with android compatibility
            if( JsonPath.Contains( "://" ) ) { // When in android
                WWW www = new WWW( JsonPath );
                //yield return www;
                while( !www.isDone ) { }

                jsonString = www.text;
                Debug.Log( "You entered the android section: " + JsonPath );
            }
            else {                        // When not in android
#if !UNITY_EDITOR && UNITY_WSA
                //HoloLens code
                Debug.Log( "You entered the Hololens section: " + JsonPath );
                try {
                    using( Stream stream = OpenFileForRead( ApplicationData.Current.RoamingFolder.Path, jsonFiles[currentTask] ) ) {
                        byte[] data = new byte[stream.Length];
                        stream.Read( data, 0, data.Length );
                        jsonString = Encoding.ASCII.GetString( data );
                    }
                }
                catch( Exception e ) {
                    Debug.Log( e );
                }
#else
                jsonString = File.ReadAllText( JsonPath );
                Debug.Log( "You entered the non-android section: " + JsonPath );
#endif
            }

            initialize( jsonString );
        }
    }
}
