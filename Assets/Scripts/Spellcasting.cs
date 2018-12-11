﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Bluetooth Controller VR Mode
// Button0 = C
// Button1 = A
// Button2 = ?

public class Spellcasting : MonoBehaviour
{
    public float timeInterval = 0.05f; // works but watch out for the value in the inspector, that's the one that counts!
    public LineRenderer lineRenderer;
    public GameObject cursor;
    public GameObject queueDrawing;
    public GameObject queueDrawing1;
    public GameObject queueDrawing2;
    public GameObject queueDrawing3;

    //Player Spells
    public GameObject fireballPrefab;
    public GameObject playerLight;
    public GameObject objectGazed;

    // Rune drawings gameobjects for queue in top right corner
    public GameObject drawingFire;
    public GameObject drawingAnimate;
    public GameObject drawingWind;


    private bool playerLightFlag = false;
    private bool telekinesisObjectFlag = false;

    private bool mouseDown = false;
    private bool drawing = false;
    private float timeSinceLastPoint;

    private List<Vector2> plist;
    private int plist_max = 5000; // max number of points in plist

    // Rune recognition variables
    private const int requiredPointCount = 20;
    private const int directionCount = requiredPointCount - 1; // max amount of queued directions
    private int patternCount; // total number of recognized runes
    private const int maxTolerance = 2;

    private int[] m_dirs;
    private List<Vector2> m_points;
    private List<int> m_indices;

    // counter clockwise, i.e right hand rule
    private enum RuneDirection
    {
        RIGHT,
        UPRIGHT,
        UP,
        UPLEFT,
        LEFT,
        DOWNLEFT,
        DOWN,
        DOWNRIGHT
    };

    enum Rune
    {
        FIRE,
        ANIMATE,
        WIND,
        WATER,
        //EARTH,
        //ICE,
        LIGHT
    };

    struct RunePattern
    {
        public Rune rune;
        public string dirs;

        public GameObject runeDrawing;

        public RunePattern(Rune r, string p, GameObject d)
        {
            rune = r;
            dirs = p;
            runeDrawing = d;
        }
    };



    RunePattern[] patternData;

   

    Rune[][] spells =
    {
        new Rune[]{Rune.FIRE},                  // Illuminate : Light up torches, candles, etc...
        new Rune[]{Rune.FIRE, Rune.ANIMATE},    // Fireball : self explanatory
        new Rune[]{Rune.ANIMATE},               // Telekinesis: manipulate objects at a distance
        new Rune[]{Rune.WATER},                 // Water: creates something to do with water
        //new Rune[]{Rune.WATER, Rune.ICE},       // Ice: Creates path of ice on someplace
        //new Rune[]{Rune.EARTH, Rune.ANIMATE},   // Earth: 
        new Rune[]{Rune.LIGHT, Rune.ANIMATE, Rune.FIRE},   // 
        //new Rune[]{Rune.EARTH, Rune.ANIMATE}


    };

    private const int MAX_RUNE_COUNT = 5;
    private int currentRune = 0;
    private List<Rune> runeQueue;

	void Start () {
        RunePattern rune1 = new RunePattern(Rune.FIRE, "602", drawingFire);
        RunePattern rune2 = new RunePattern(Rune.ANIMATE, "0501", drawingAnimate);
        RunePattern rune3 = new RunePattern(Rune.WIND, "171", drawingAnimate);
        RunePattern rune4 = new RunePattern(Rune.WATER, "7171", drawingAnimate);
        RunePattern rune5 = new RunePattern(Rune.LIGHT, "605", drawingAnimate);

        patternData = new RunePattern[5];
        patternData[0] = rune1;
        patternData[1] = rune2;
        patternData[2] = rune3;
        patternData[3] = rune4;
        patternData[4] = rune5;
        Debug.Log(patternData);

        patternCount = patternData.Length;
        plist = new List<Vector2>();
        m_dirs = new int[directionCount];
        m_points = new List<Vector2>();
        m_indices = new List<int>();
        runeQueue = new List<Rune>();

        cursor.SetActive(false);
        queueDrawing.SetActive(false);

        timeSinceLastPoint = timeInterval; // first click counts as the first point
    }

    /* ==============================================================================
        Point processing
       ============================================================================== */

    /*!
    * Find all key points in the input (angles greater than minAngle).
    * The output will always be at least two points, the start and the end.
    * Inserts indices pointing to the in vector into m_indices.
    */
    void findKeyPoints(List<Vector2> input)
    {
        const float TOLERANCE = 0.30f;
        int inputSize = input.Count;

        // Calculate tolerance based on the overall size of the drawing
        Vector2 max = input[0];
        Vector2 min = input[0];
        for(int i = 1; i < inputSize; i++)
        {
            max = Vector2.Max(max, input[i]);
            min = Vector2.Min(min, input[i]);
        }

        // distance tolerance, i.e ignore points that are too close
        float currTolerance = (max.x - min.x + max.y - min.y) / 2.0f * TOLERANCE;

        m_indices.Add(0);
        Vector2 lastImp = input[0];


        float minAngle = (2.0f / 3.0f) * Mathf.PI;

        for(int i = 2; i < inputSize; i++)
        {
            Vector2 thisPoint = input[i - 1];
            Vector2 nextPoint = input[i];

            float distance = (lastImp - thisPoint).magnitude;
            if(distance > currTolerance)
            {
                float angle2 = Vector2.Angle((lastImp - thisPoint).normalized, (nextPoint - thisPoint).normalized) * Mathf.Deg2Rad;
                if(angle2 < minAngle)
                {
                    lastImp = thisPoint;
                    m_indices.Add(i - 1);
                }
            }
        }
        m_indices.Add(inputSize - 1);
    }

    /*!
     * Resample the input while keeping key points contained in m_indices
     * Based on the resample function of $1 Unistroke Recognizer
     * https://depts.washington.edu/aimgroup/proj/dollar/
     */
    void resampleInput(List<Vector2> input)
    {
        float totalLen = 0.0f;
        for(int i = 1; i < input.Count; i++)
        {
            totalLen += Vector2.Distance(input[i - 1], input[i]);
        }

        m_points.Add(input[0]);

        int segmentCount = m_indices.Count - 1;
        int pointsAdded = 0;
        float segRemains = 0.0f;

        for(int segment = 0; segment < segmentCount; segment++)
        {
            // Distance along curve from key point 1 to key point 2
            float segLen = 0.0f;
            for(int i = m_indices[segment]; i < m_indices[segment + 1]; i++)
            {
                segLen += Vector2.Distance(input[i], input[i + 1]);
            }

            float pointsToAddFloat = (segLen / totalLen) * (requiredPointCount - 1) + segRemains;
            int pointsToAdd = (int)(Mathf.Round(pointsToAddFloat));
            segRemains = pointsToAddFloat - (float)(pointsToAdd);

            if(segment != segmentCount - 1)
            {
                pointsAdded += pointsToAdd;
            }
            else
            {
                pointsToAdd = requiredPointCount - 1 - pointsAdded;
            }

            if (pointsToAdd == 0)
                continue;

            Debug.Assert(pointsToAdd > 0);

            float interval = segLen / pointsToAdd;
            float remains = 0.0f;

            bool newPointAdded = false; // Was a new point added?
            bool endOfSegment = false; // At the end of segment

            int index = m_indices[segment] + 1;
		    int reallyAdded = 0;
		    int endIndex = m_indices[segment + 1];
		
		    Vector2 prevPoint = input[m_indices[segment]];
		    Vector2 thisPoint = new Vector2(0.0f, 0.0f); // Rafael - Not sure if I copied this correctly

            while(!endOfSegment || newPointAdded)
            {
                if (!newPointAdded)
                {
                    if (!endOfSegment)
                    {
                        thisPoint = input[index];
                        if(index == endIndex)
                        {
                            endOfSegment = true;
                        }
                        else
                        {
                            index++;
                        }
                    }
                }
                else
                {
                    newPointAdded = false;
                }

                float dist = Vector2.Distance(prevPoint, thisPoint);
                float coeff = (interval - remains) / dist;
                if((remains + dist) >= interval)
                {
                    Vector2 p = prevPoint + coeff * (thisPoint - prevPoint);
                    m_points.Add(p);
                    reallyAdded++;
                    remains = 0.0f;
                    prevPoint = p;
                    newPointAdded = true;
                    continue;
                }
                else
                {
                    remains += dist;
                }

                prevPoint = thisPoint;
            } // end of while

            if(reallyAdded == pointsToAdd - 1)
            {
                // Fell short of one point due to rounding error
                m_points.Add(input[endIndex]);
            }
        }
    }


    /*!
    * Return the angle between a vector and the X axis in radians
    * The angle is always positive
    */
    float angleVectorX(Vector2 v)
    {
        float angle = -(Vector2.SignedAngle(v.normalized, new Vector2(1.0f, 0.0f))*Mathf.Deg2Rad);

        if (angle < 0)
            angle += 2 * Mathf.PI;

        return angle;
    }

    /*!
     * Round the angle to the nearest direction
     */
    int quantizeAngleToDir(float angle)
    {
        // 8 slices means 8 quarter circles = slice is pi/4
        return (int)(Mathf.Round(angle / (Mathf.PI / 4.0f)) % 8);
    }

    /*!
     * Convert a vector of input points to a sequence of directions
     */
    void inputToDirs()
    {
        for(int i = 1; i < m_points.Count; i++)
        {
            int dir = quantizeAngleToDir(angleVectorX(m_points[i] - m_points[i - 1]));
            Debug.Assert(dir >= 0 && dir < 8);
            m_dirs[i - 1] = dir;
        }
    }

    /*!
     * Return the smallest difference between angle1 and angle2
     */
    int angleDiff(int angle1, int angle2)
    {
        return (4 - Mathf.Abs(Mathf.Abs(angle1 - angle2) - 4));
    }


    /*! Compare the input directions (m_dirs) with each rune pattern
     *  Returns an index to patternData, -1 if no match is found
     */
    int findMatchingPattern()
    {
        int index = -1;
        int min = int.MaxValue;
        for(int rune = 0; rune < patternCount; rune++)
        {
            bool refuse = false;
            int errors = 0;
            int patternIndex = 0, inputIndex = 0;
            int patternSize = patternData[rune].dirs.Length;

            int curPatternDir = (int)char.GetNumericValue(patternData[rune].dirs[0]);
            int curInputDir = m_dirs[0];
            int nextPatternDir = (patternIndex < patternSize - 1) ? (int)char.GetNumericValue(patternData[rune].dirs[1]) : curPatternDir ;
            int nextInputDir = (inputIndex < directionCount - 1) ? m_dirs[1] : curInputDir ;

            while (inputIndex < directionCount)
            {
                int diff = angleDiff(curPatternDir, curInputDir);
                errors += diff;

                if(diff > 1 || errors > maxTolerance)
                {
                    refuse = true;
                    break;
                }

                if(patternIndex < patternSize - 1  && nextInputDir >= 0 && nextInputDir != curInputDir)
                {
                    // If the pattern deviates, move to the next pattern dir only if the difference is smaller
                    if (nextPatternDir >= 0 && (nextPatternDir == nextInputDir
                        || angleDiff(nextInputDir, curPatternDir) > angleDiff(nextInputDir, nextPatternDir)))
                    {
                        patternIndex++;
                        curPatternDir = nextPatternDir;
                        nextPatternDir = (patternIndex < patternSize - 1) ? (int)char.GetNumericValue(patternData[rune].dirs[patternIndex + 1]) : curPatternDir;
                    }
                }

                curInputDir = nextInputDir;
                nextInputDir = (inputIndex < directionCount - 1) ? m_dirs[inputIndex + 1] : curInputDir;
                inputIndex++;
            }

            if (patternIndex < patternSize - 1 || refuse)
            {
                continue;
            }

            if (errors < min)
            {
                min = errors;
                index = rune;
            }
        }

        if (min <= maxTolerance)
        {
            return index;
        }
        else
        {
            return -1;
        }
    }

    void runeDetection(Rune rune)
    {
        if (currentRune >= MAX_RUNE_COUNT)
        {
            runeQueue[2] = runeQueue[1];
            runeQueue[1] = runeQueue[0];
            runeQueue.Insert(0, rune);
        }
        else
        {
            runeQueue.Add(rune);
        }
    }
    void runeQueueDrawing(GameObject runedraw)
    {

        queueDrawing3.GetComponent<MeshFilter>().mesh = queueDrawing2.GetComponent<MeshFilter>().mesh;
        queueDrawing3.GetComponent<MeshRenderer>().material = queueDrawing2.GetComponent<MeshRenderer>().material;
        queueDrawing3.GetComponent<Transform>().rotation = queueDrawing2.GetComponent<Transform>().rotation;
        queueDrawing3.GetComponent<Transform>().localScale = queueDrawing2.GetComponent<Transform>().localScale;


        queueDrawing2.GetComponent<MeshFilter>().mesh = queueDrawing1.GetComponent<MeshFilter>().mesh;
        queueDrawing2.GetComponent<MeshRenderer>().material = queueDrawing1.GetComponent<MeshRenderer>().material;
        queueDrawing2.GetComponent<Transform>().rotation = queueDrawing1.GetComponent<Transform>().rotation;
        queueDrawing2.GetComponent<Transform>().localScale = queueDrawing1.GetComponent<Transform>().localScale;

        queueDrawing1.GetComponent<MeshFilter>().mesh = runedraw.GetComponentInChildren<MeshFilter>().mesh;
        queueDrawing1.GetComponent<MeshRenderer>().material = runedraw.GetComponentInChildren<MeshRenderer>().material;
        queueDrawing1.GetComponent<Transform>().rotation = runedraw.GetComponent<Transform>().rotation;
        queueDrawing1.GetComponent<Transform>().localScale = runedraw.GetComponent<Transform>().localScale;
       
    }

    string runeToString(Rune rune)
    {
        string result;
        switch (rune)
        {
            case Rune.FIRE:
                result = "Fire";
                break;
            case Rune.ANIMATE:
                result = "Animate";
                break;
            case Rune.WIND:
                result = "Wind";
                break;
            case Rune.WATER:
                result = "Water";
                break;
            case Rune.LIGHT:
                result = "Light";
                break;
           /* case Rune.EARTH:
                result = "Earth";
                break;
            case Rune.ICE:
                result = "Ice";
                break;*/
            default:
                result = "You forgot to add it!";
                break;
           
        }

        return result;
    }

    void runeHandler(int index)
    {
        runeDetection(patternData[index].rune);
        runeQueueDrawing(patternData[index].runeDrawing);
        Debug.Log("Rune recognized: " + runeToString(patternData[index].rune));
    }

    void illuminate()
    {
        if (objectGazed.CompareTag("TorchLighting")){
               objectGazed.transform.Find("Torch Lighting").gameObject.SetActive(!objectGazed.transform.Find("Torch Lighting").gameObject.activeSelf);
        }
    }

    public void water()
    {
        if (objectGazed.CompareTag("Barrel"))
        {
            if (objectGazed.GetComponent<BarrelControler>().imHit)
            {
                Destroy(objectGazed.gameObject);
            }
        }
    }

    void createLight()
    {
        playerLightFlag = true;
        playerLight.SetActive(true);
       
    }
    void Telekinesis() {
        if (objectGazed != null) {
            if (objectGazed.CompareTag("Barrel")){
                objectGazed.GetComponent<Rigidbody>().isKinematic = !objectGazed.GetComponent<Rigidbody>().isKinematic;
                telekinesisObjectFlag = !telekinesisObjectFlag;
            }
            if (objectGazed.CompareTag("DoorPuzzle1")) {
                Debug.Log("ENTREI IHHIHI");
                objectGazed.transform.localPosition = new Vector3(11.542f, -2.746f, 1.577f);
                objectGazed.transform.localEulerAngles = new Vector3(0, -180,0);
            }
        }

        
    }

    public void AssignGazedObject(GameObject receivedObject) {
        objectGazed = receivedObject;
    }
    public void RemoveGazedObject() {
        if (!telekinesisObjectFlag)
        {
            objectGazed = null;
        }
    }

    void fireball()
    {
        GameObject clone = Instantiate(fireballPrefab, transform.position, Quaternion.identity);
        clone.GetComponent<SpellControler>().direction = Camera.main.transform.forward;
    }

    void castSpell(int index)
    {
        // switch case for each spell...
        switch (index)
        {
            case 0: // Illuminate
                Debug.Log("Illuminate cast!");
                illuminate();
                break;
            case 1: // Fireball
                Debug.Log("Fireball cast!");
                fireball();
                break;
            case 2:
                Debug.Log("Telekenisis cast!");
                Telekinesis();
                break;
            case 3:
                Debug.Log("Telekenisis cast!");
                water();
                break;
            case 4:
                Debug.Log("Light cast!");
                createLight();
                break;
            default:
                Debug.Log("Unknown spell cast?");
                break;
        }
    }

    void invokeSpell()
    {
        int runeSeqLen = runeQueue.Count;
        List<int> candidates = new List<int>();

        // check for spells of the correct length
        for(int i = 0; i < spells.Length; i++)
        {
            if(spells[i].Length == runeSeqLen)
            {
                candidates.Add(i);
            }
        }

        //Debug.Log("Candidates length: " + candidates.Count.ToString());

        // for each rune, check for each candidate 
        for(int rune = 0; rune < runeSeqLen; rune++)
        {
            for(int i = 0; i < candidates.Count; i++)
            {
                //Debug.Log("Entered for loop...");
                //Debug.Log("If statement value: " + (runeQueue[rune] != spells[candidates[i]][rune]));
                if(runeQueue[rune] != spells[candidates[i]][rune])
                {
                    candidates.Remove(candidates[i]);
                }
            }
        }

        if (candidates.Count == 0)
        {
            // SPELL FIZZLED! NO KNOWN SPELL MATCHES RUNES!
            Debug.Log("Spell cast fizzled!");
        }
        else if(candidates.Count == 1)
        {
            // SPELL CAST SUCCESSFUL!
            castSpell(candidates[0]);
            Debug.Log("Spell cast successful!");
        }
        else
        {
            // THIS SHOULD NEVER HAPPEN BUT, MORE THAN 1 SPELL FOUND!
            Debug.Log("WTF LOL");
        }

    }

    /*==============================================================================*/
    void analyze ()
    {
        if(plist.Count < 2)
        {
            plist.Clear();
            Debug.Log("Too few points!");
            return;
        }

        m_points.Clear();
        m_indices.Clear();

        findKeyPoints(plist);
        if (m_indices.Count > requiredPointCount)
        {
            Debug.Log("Too deformed!");
            // too deformed
            return;
        }

        resampleInput(plist);
        inputToDirs();

        /**/
        int index = findMatchingPattern();

        if(index >= 0)
        {
            runeHandler(index);
        }
        else
        {
            return;
        }

        /**/

        // precast stuff here

    }


    /* ==============================================================================
        Main loop
       ============================================================================== */

    void Update () {

        if (telekinesisObjectFlag == true)
        {

            objectGazed.transform.position = transform.position + Camera.main.transform.forward * 4.0f;
        }
        
        if (playerLightFlag == true){
            
            playerLight.GetComponent<Light>().intensity -= Time.deltaTime;
            if (playerLight.GetComponent<Light>().intensity <= 0){
                playerLight.SetActive(false);
                playerLightFlag = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            if (!drawing)
            {
                drawing = true;
                cursor.SetActive(true);
                //cursor.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2.0f , Screen.height / 2.0f, 1.0f));
                cursor.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1.0f;
                cursor.transform.rotation = Camera.main.transform.rotation;
                queueDrawing.SetActive(true);
               
            }
            else
            {
                drawing = false;
                cursor.SetActive(false);
                queueDrawing.SetActive(false);
                queueDrawing3.GetComponent<MeshFilter>().mesh.Clear();
                queueDrawing2.GetComponent<MeshFilter>().mesh.Clear();
                queueDrawing1.GetComponent<MeshFilter>().mesh.Clear();

                if (mouseDown)
                {
                    mouseDown = false;
                    plist.Clear();
                    lineRenderer.positionCount = 0;
                    m_dirs.Initialize();
                    Debug.Log("Spell casting hard cancelled!");
                    runeQueue.Clear();
                }
                else
                {
                    // cast spell here
                    invokeSpell();
                    runeQueue.Clear();
                }
            }
        }
 
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton0)) && !mouseDown)
        {
            mouseDown = true;
        }
        else if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.JoystickButton0)) && mouseDown)
        {
            mouseDown = false;

            analyze();

            plist.Clear();
            lineRenderer.positionCount = 0;
            m_dirs.Initialize();

            string log = "";
            foreach(Rune rune in runeQueue)
            {
                log += runeToString(rune) + " ";
            }
            Debug.Log("Rune queue: " + log);
        }

        if (drawing)
        {
            // move cursor from input
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            Vector3 horizontal = cursor.transform.right * h * 0.015f;
            Vector3 vertical = cursor.transform.up * v * 0.015f;

            cursor.transform.position += horizontal + vertical;

            if (mouseDown)
            {
          


                if (timeSinceLastPoint >= timeInterval)
                {
                    // use cursor position in world
                    Vector2 mousePos = Camera.main.WorldToScreenPoint(cursor.transform.position);

                    if(plist.Count < plist_max)
                    {
                        plist.Add(mousePos);
                        timeSinceLastPoint = 0.0f;

                        //rendering stuff
                        lineRenderer.positionCount += 1;
                        lineRenderer.SetPosition(plist.Count - 1, cursor.transform.position);
                    }
                    
                }
                else
                {
                    timeSinceLastPoint += Time.deltaTime;
                }
            }
        }

    }
}
