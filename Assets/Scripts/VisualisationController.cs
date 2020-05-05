using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Concurrent;
using UnityEngine.SceneManagement;
using System.Linq;

public class VisualisationController : MonoBehaviour
{
    public GameObject grainPrefab;

    public Text maxValueText;
    public Text mainButtonLabel;
    public Button ModeButton;
    public Button MCStep;
    public GameObject MCSubscreen;

    public GameObject energyBar;
    public GameObject parentObject;
    public GameObject MCParentObject;

    public GameObject[,] objects;
    public GameObject[,] MCobjects;

    private string boundaryMethod;
    private string nucleationMethod;
    private string neighborhoodMethod;

    private DisplayMode actualMode;
    private float radiusNucleation;
    private float radiusNeighborhood;
    private int actualIteration;
    private Color32 colorBlack;
    private List<GameObject> list;
    private int widthTotal;
    private int heightTotal;
    private int iterationMax;
    private float ktParameter;
    private List<Point> temppoints;

    private Color32[,] actualColor;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }
    }
    private void OnEnable()
    {

        actualMode = DisplayMode.STRUCTURE;
        actualIteration = 0;
        widthTotal = PlayerPrefs.GetInt("width");
        heightTotal = PlayerPrefs.GetInt("height");

        iterationMax = PlayerPrefs.GetInt("iterationMax");
        ktParameter = PlayerPrefs.GetFloat("kt");
        nucleationMethod = PlayerPrefs.GetString("nucleationMethod");
        boundaryMethod = PlayerPrefs.GetString("boundaryMethod");
        neighborhoodMethod = PlayerPrefs.GetString("neighborhoodMethod");
        radiusNucleation = PlayerPrefs.GetFloat("radiusNucleation");
        radiusNeighborhood = PlayerPrefs.GetFloat("radiusNeighborhood");

        objects = new GameObject[heightTotal, widthTotal];
        MCobjects = new GameObject[heightTotal, widthTotal];
        actualColor = new Color32[heightTotal, widthTotal];

        list = new List<GameObject>();
        temppoints = new List<Point>();
        colorBlack = new Color32((byte)0, (byte)0, (byte)0, (byte)255);
        SetCellSizeAndCanvas();
        InitiateCells();
        StartLife(nucleationMethod);
    }

    public void ReloadPrefs()
    {
        iterationMax = PlayerPrefs.GetInt("iterationMax");
        ktParameter = PlayerPrefs.GetFloat("kt");
        nucleationMethod = PlayerPrefs.GetString("nucleationMethod");
        boundaryMethod = PlayerPrefs.GetString("boundaryMethod");
        neighborhoodMethod = PlayerPrefs.GetString("neighborhoodMethod");
        radiusNeighborhood = PlayerPrefs.GetFloat("radiusNeighborhood");
    }

    public void OnModeToggle()
    {
        if (actualMode.Equals(DisplayMode.ENERGY))
        {
            energyBar.SetActive(false);
            for (int y = 0; y < heightTotal; y++)
            {
                for (int x = 0; x < widthTotal; x++)
                {
                    objects[y, x].GetComponent<Image>().color = actualColor[y, x];
                }
            }
            
            actualMode = DisplayMode.STRUCTURE;
        }
        else if (actualMode.Equals(DisplayMode.STRUCTURE))
        {
            int tempMaxEnergy = 1;
            energyBar.SetActive(true);
            // Looking for max energy to scale up
            for (int y = 0; y < heightTotal; y++)
            {
                for (int x = 0; x < widthTotal; x++)
                {
                    if (objects[y, x].GetComponent<Grain>().energy > tempMaxEnergy)
                    {
                        tempMaxEnergy = objects[y, x].GetComponent<Grain>().energy;
                    }
                }
            }
            maxValueText.text = tempMaxEnergy.ToString();

            for (int y = 0; y < heightTotal; y++)
            {
                for (int x = 0; x < widthTotal; x++)
                {
                    objects[y, x].GetComponent<Image>().color = ColorHandler.MapEnergyToColor(objects[y, x].GetComponent<Grain>().energy,tempMaxEnergy);
                }
            }
            actualMode = DisplayMode.ENERGY;
        }
    }

    public void OnMonteCarloClick()
    {
        if (actualIteration < iterationMax)
        {
            Debug.Log("Monte Carlo iteration: " + actualIteration);

            List<Point> neighborPoints = new List<Point>() ;

            // Optimal method of randomizing points sequence

            List<Point> randomPoints = new List<Point>();

            for (int y = 0; y < heightTotal; y++)
            {
                for (int x = 0; x < widthTotal; x++)
                {
                    randomPoints.Add(new Point(x, y, widthTotal, heightTotal, boundaryMethod));
                }
            }

            randomPoints = randomPoints.OrderBy(i => System.Guid.NewGuid()).ToList();

            foreach (Point point in randomPoints)
            {
                if (neighborhoodMethod.Equals("Moore"))
                {
                    neighborPoints = MooreNeighborhood(point);
                }
                else if (neighborhoodMethod.Equals("Von Neumann"))
                {
                    neighborPoints = VonNeumanNeighborhood(point);
                }
                else if (neighborhoodMethod.Equals("Hexa Random"))
                {
                    neighborPoints = HexNeighborhoodRandom(point);
                }
                else if (neighborhoodMethod.Equals("Hexa Left"))
                {
                    neighborPoints = HexNeighborhoodLeft(point);
                }
                else if (neighborhoodMethod.Equals("Hexa Right"))
                {
                    neighborPoints = HexNeighborhoodRight(point);
                }
                else if (neighborhoodMethod.Equals("Penta Random"))
                {
                    neighborPoints = PentaRandomNeighborhood(point);
                }
                else if (neighborhoodMethod.Equals("Radius"))
                {
                    neighborPoints = RadiusNeighborhood(point);
                }

                if (boundaryMethod.Equals("Absorbing") && !neighborhoodMethod.Equals("Radius")) // Delete out of space for absorbic radius is executed in RadiusNeighborhood method
                {
                    neighborPoints = DeleteOutOutOfSpace(neighborPoints);
                }

                CalculateEnergy(point, neighborPoints);
            }
            actualIteration++;

            Debug.Log("End of Monte Carlo step");
        }
    }

    private void CalculateEnergy(Point me, List<Point> neighborhoodPoints)
    {
        int beforeEnergy = 0;
        int afterEnergy = 0;
        int deltaEnergy;
        double probability;
        ConcurrentDictionary<Color32, int> colorDictionary = new ConcurrentDictionary<Color32, int>();
        List<Color32> listOfOtherColors = new List<Color32>();

        foreach (Point point in neighborhoodPoints)
        {
            colorDictionary.AddOrUpdate(actualColor[point.y,point.x], 1, (id, count) => count + 1);
        }

        foreach (KeyValuePair<Color32, int> entry in colorDictionary)
        {
            if (entry.Key.Equals(actualColor[me.y,me.x]))
            {

            }
            else
            {
                // Calculate sum of other colors
                listOfOtherColors.Add(entry.Key);
                beforeEnergy += entry.Value;
            }
        }
        if (beforeEnergy == 0) // There aren't any other color in neighborhood. Trying to change isn't a good idea.
        {
            objects[me.y, me.x].GetComponent<Grain>().energy = 0;
            return;
        }

        Color32 newColorAttempt = listOfOtherColors[Random.Range(0, listOfOtherColors.Count)];

        foreach (KeyValuePair<Color32, int> entry in colorDictionary)
        {
            if (entry.Key.Equals(newColorAttempt))
            {

            }
            else
            {
                // Calculate sum of colors diffrent that attemption color
                afterEnergy += entry.Value;
            }
        }

        deltaEnergy = afterEnergy - beforeEnergy;

        if (deltaEnergy <= 0)
        {
            probability = 1d;
        }
        else
        {
            double temp = deltaEnergy / ktParameter;
            temp *= -1;
            probability = System.Math.Exp(temp);

        }

        if (Random.Range(0f, 1f) < (float)probability)
        {
            //Debug.Log("Change");
            objects[me.y, me.x].GetComponent<Grain>().energy = afterEnergy;
            objects[me.y, me.x].GetComponent<Image>().color = newColorAttempt;
            actualColor[me.y, me.x] = newColorAttempt;
        }
        else
        {
            //Debug.Log("Dont change");
            objects[me.y, me.x].GetComponent<Grain>().energy = beforeEnergy;
        }
    }

    private bool EndOfFreeSpace()
    {
        for (int y = 0; y < heightTotal; y++)
        {
            for (int x = 0; x < widthTotal; x++)
            {
                if (actualColor[y, x].Equals(colorBlack))
                {
                    Debug.Log("The microstructure hasn't generated yet!");
                    return false;
                }
            }
        }
        return true;
    }

    private void SetCellSizeAndCanvas()
    {
        float canvasWidth = parentObject.GetComponent<RectTransform>().sizeDelta.x;
        float canvasHeight = parentObject.GetComponent<RectTransform>().sizeDelta.y;
        float cellX = canvasWidth / widthTotal;
        float cellY = canvasHeight / heightTotal;
        float squareCellSide;

        //Cells need to be a squares with lower values (to fill canvas)
        if (cellX < cellY)
        {
            squareCellSide = cellX;
        }
        else
        {
            squareCellSide = cellY;
        }
        //Cell size is well counted
        parentObject.GetComponent<GridLayoutGroup>().cellSize = new Vector2(squareCellSide, squareCellSide);
        MCParentObject.GetComponent<GridLayoutGroup>().cellSize = new Vector2(squareCellSide, squareCellSide);
        

        //Recalculate width and height of canvas
        parentObject.GetComponent<RectTransform>().sizeDelta = new Vector2((widthTotal * squareCellSide), (heightTotal * squareCellSide));
        MCParentObject.GetComponent<RectTransform>().sizeDelta = new Vector2((widthTotal * squareCellSide), (heightTotal * squareCellSide));
    }
    
    public void UpdateColorBase()
    {
        for (int y = 0; y < heightTotal; y++)
        {
            for (int x = 0; x < widthTotal; x++)
            {
                actualColor[y, x] = objects[y, x].GetComponent<Image>().color;
            }
        }
    }

    private void InitiateCells()
    {
        for (int y = 0; y < heightTotal; y++)
        {
            for (int x = 0; x < widthTotal; x++)
            {
                objects[y,x] = Instantiate(grainPrefab, parentObject.transform);
                objects[y,x].GetComponent<Grain>().position = new Vector2(x, -y) ;
            }
        }
    }

    public void NextStep()
    {
        
        Color32[,] newColors = actualColor.Clone() as Color32[,];

        for (int y = 0; y < heightTotal; y++)
        {
            for (int x = 0; x < widthTotal; x++)
            {
                if (actualColor[y, x].Equals(colorBlack))
                {
                    newColors[y, x] = BestColorInNeighborhood(new Point(x, y, widthTotal, heightTotal, boundaryMethod));
                }
            }
        }

        for (int y = 0; y < heightTotal; y++)
        {
            for (int x = 0; x < widthTotal; x++)
            {
                objects[y, x].GetComponent<Image>().color = newColors[y, x];    //podstawienie
                actualColor[y, x] = newColors[y, x];
            }
        }

        if (EndOfFreeSpace())
        {
            MCStep.interactable = true;
            Debug.Log("Microstructure is already created! Try to Monte Carlo method!");
        }
    }

    private Color32 BestColorInNeighborhood(Point me)
    {
        List<Point> points = new List<Point>();
        List<Color32> bestColors = new List<Color32>();
        ConcurrentDictionary<Color32, int> colorDictionary = new ConcurrentDictionary<Color32, int>();
        Color32 tempColor = colorBlack;
        bool allBlack = true;
        int max = 0;
        Color32 bestColor = colorBlack ;

        if (neighborhoodMethod.Equals("Moore"))
        {
            points = MooreNeighborhood(me);
        }
        else if (neighborhoodMethod.Equals("Von Neumann"))
        {
            points = VonNeumanNeighborhood(me);
        }
        else if (neighborhoodMethod.Equals("Hexa Random"))
        {
            points = HexNeighborhoodRandom(me);
        }
        else if (neighborhoodMethod.Equals("Hexa Left"))
        {
            points = HexNeighborhoodLeft(me);
        }
        else if (neighborhoodMethod.Equals("Hexa Right"))
        {
            points = HexNeighborhoodRight(me);
        }
        else if (neighborhoodMethod.Equals("Penta Random"))
        {
            points = PentaRandomNeighborhood(me);
        }
        else if (neighborhoodMethod.Equals("Radius"))
        {
            points = RadiusNeighborhood(me);
        }

        if (boundaryMethod.Equals("Absorbing")&&!neighborhoodMethod.Equals("Radius")) // Delete out of space for absorbic radius is executed in RadiusNeighborhood method
        {
            points = DeleteOutOutOfSpace(points);
        }

        foreach (Point point in points)
        {
            tempColor = actualColor[point.y, point.x]; 
            if (tempColor.Equals(colorBlack))
            {

            }
            else
            {
                colorDictionary.AddOrUpdate(tempColor, 1, (id, count) => count + 1);
                allBlack = false;
            }
        }

        if (allBlack)
        {
            return colorBlack;
        }
        else
        {
            foreach (KeyValuePair<Color32, int> entry in colorDictionary)
            {
                if (entry.Value > max)
                {
                    bestColors.Clear();
                    bestColors.Add(entry.Key);
                }
                else if (entry.Value == max)
                {
                    bestColors.Add(entry.Key);
                }
            }
        }

        if (allBlack)
        {
            return colorBlack;
        }
        else
        {
            return bestColors[Random.Range(0, bestColors.Count)];
        }

    }

    private List<Point> DeleteOutOutOfSpace(List<Point> points)
    {
        List<Point> newTempPoint = new List<Point>();
        foreach (Point point in points)
        {
            if (point.x != -9999 && point.y != -9999)
            {
                newTempPoint.Add(point);
            }
        }
        return newTempPoint;
    }

    private List<Point> PentaRandomNeighborhood(Point me)
    {
        int choose=Random.Range(0, 4);
        switch (choose)
        {
            case 0:
                {
                    return PentaRight(me);
                }
            case 1:
                {
                    return PentaLeft(me);
                }
            case 2:
                {
                    return PentaTop(me);
                }
            default:
                {
                    return PentaBottom(me);
                }
        }
    }

    private List<Point> RadiusNeighborhood(Point me)
    {
        temppoints.Clear();
        int iCeil = (int)System.Math.Ceiling((double)radiusNeighborhood);
        int jCeil = (int)System.Math.Ceiling((double)radiusNeighborhood);

        for (int i = -iCeil; i < iCeil; i++)
        {
            for (int j= -jCeil; j < jCeil; j++)
            {
                temppoints.Add(new Point(me.x + i, me.y + j, widthTotal, heightTotal, boundaryMethod));
            }
        }
        // Delete out of range points.
        if (boundaryMethod.Equals("Absorbing"))
        {
            temppoints = DeleteOutOutOfSpace(temppoints);
        }
        return DeleteOutOfRange(me);
    }

    private List<Point> DeleteOutOfRange(Point me)
    {
        List<Point> newTempPoint = new List<Point>();
        float x;
        float y;
        foreach (Point point in temppoints)
        {
            // Cannot modify values of list
            x = objects[point.y, point.x].GetComponent<Grain>().position.x;
            y = objects[point.y, point.x].GetComponent<Grain>().position.y;

            if (point.xPassed.Equals(XPassed.BY_LEFT))
            {
                x = x - widthTotal;
            }
            else if(point.xPassed.Equals(XPassed.BY_RIGHT))
            {
                x = x + widthTotal;
            }

            if (point.yPassed.Equals(YPassed.BY_UPPER))
            {
                y = y + heightTotal;
            }
            else if (point.yPassed.Equals(YPassed.BY_LOWER))
            {
                y = y - heightTotal;
            }

            if (CalculateDistance(new Vector2(x,y), objects[me.y, me.x].GetComponent<Grain>().position) < radiusNeighborhood)
            {
                newTempPoint.Add(point);
            }
        }
        return newTempPoint;
    }

    private List<Point> PentaRight(Point me)
    {
        temppoints.Clear();
        temppoints.Add(new Point(me.x+1, me.y, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x+1, me.y-1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x+1, me.y+1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x, me.y-1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x, me.y+1, widthTotal, heightTotal, boundaryMethod));
        return temppoints;
    }

    private List<Point> PentaLeft(Point me)
    {
        temppoints.Clear();
        temppoints.Add(new Point(me.x - 1, me.y, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x - 1, me.y - 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x - 1, me.y + 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x, me.y - 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x, me.y + 1, widthTotal, heightTotal, boundaryMethod));
        return temppoints;
    }

    private List<Point> PentaTop(Point me)
    {
        temppoints.Clear();
        temppoints.Add(new Point(me.x, me.y+1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x-1, me.y+1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x +1, me.y + 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x-1, me.y, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x+1, me.y, widthTotal, heightTotal, boundaryMethod));
        return temppoints;
    }

    private List<Point> PentaBottom(Point me)
    {
        temppoints.Clear();
        temppoints.Add(new Point(me.x, me.y - 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x - 1, me.y - 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x + 1, me.y - 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x - 1, me.y, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x + 1, me.y, widthTotal, heightTotal, boundaryMethod));
        return temppoints;
    }

    private List<Point> MooreNeighborhood(Point me)
    {
        temppoints.Clear();
        temppoints.Add(new Point(me.x - 1, me.y - 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x , me.y - 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x + 1, me.y - 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x - 1, me.y, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x + 1, me.y, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x - 1, me.y + 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x , me.y + 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x + 1, me.y + 1, widthTotal, heightTotal, boundaryMethod));
        return temppoints;
    }

    private List<Point> VonNeumanNeighborhood(Point me)
    {
        temppoints.Clear();
        temppoints.Add(new Point(me.x - 1, me.y, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x + 1, me.y, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x, me.y - 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x, me.y + 1, widthTotal, heightTotal, boundaryMethod));

        return temppoints;
    }

    private List<Point> HexNeighborhoodRandom(Point me)
    {
        int choose = Random.Range(0, 2);
        if (choose == 0)
        {
            return HexNeighborhoodLeft(me);
        }
        else
        {
            return HexNeighborhoodRight(me);
        }
    }

    private List<Point> HexNeighborhoodLeft(Point me)
    {
        temppoints.Clear();
        temppoints.Add(new Point(me.x, me.y + 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x+1, me.y +1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x+1, me.y, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x-1, me.y, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x-1, me.y - 1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x, me.y - 1, widthTotal, heightTotal, boundaryMethod));

        return temppoints;
    }

    private List<Point> HexNeighborhoodRight(Point me)
    {
        temppoints.Clear();
        temppoints.Add(new Point(me.x-1, me.y+1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x-1, me.y, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x, me.y+1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x, me.y-1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x+1, me.y-1, widthTotal, heightTotal, boundaryMethod));
        temppoints.Add(new Point(me.x+1, me.y, widthTotal, heightTotal, boundaryMethod));
        return temppoints;
    }



    private void StartLife(string nucleationMethod)
    {
        for (int y = 0; y < heightTotal; y++)
        {
            for (int x = 0; x < widthTotal; x++)
            {
                actualColor[y, x] = colorBlack;
            }
        }

        switch (nucleationMethod)
        {
            case "Plain":
                {
                    int biasX = (int)widthTotal / PlayerPrefs.GetInt("widthUnits");
                    int biasY = (int)heightTotal / PlayerPrefs.GetInt("heightUnits");
                    for (int y = 0; y < PlayerPrefs.GetInt("heightUnits"); y++)
                    {
                        for (int x = 0; x < PlayerPrefs.GetInt("widthUnits"); x++)
                        {
                            objects[y * biasY, x * biasY].GetComponent<Image>().color = ColorHandler.GenerateColor(); //podstawienie
                        }
                    }
                    break;
                }
            case "Random":
                {
                    for (int i = 0; i < PlayerPrefs.GetInt("randomUnits"); i++)
                    {
                        for (; ; )
                        {
                            int x = (int)Random.Range(0, widthTotal);
                            int y = (int)Random.Range(0, heightTotal);
                            if (actualColor[y, x].Equals(colorBlack))
                            {
                                objects[y, x].GetComponent<Image>().color = ColorHandler.GenerateColor();  //podstawienie
                                break;
                            }
                        }
                    }
                    break;
                }
            case "Radius":
                {
                    for (int i = 0; i < PlayerPrefs.GetInt("amountNucleation"); i++)
                    {
                        for (int j = 0; j < 500; j++)
                        {
                            int x = (int)Random.Range(0, widthTotal);
                            int y = (int)Random.Range(0, heightTotal);
                            if (!CheckIfAnyInRadius(objects[y, x]))
                            {
                                objects[y, x].GetComponent<Image>().color = ColorHandler.GenerateColor();  //Podstawienie
                                list.Add(objects[y, x]);
                                break;
                            }

                            if (j == 499) // Too many tries. Give up
                            {
                                Debug.LogError("Box is full! Cannot add any grain with that radius!");
                                return;
                            }
                        }
                    }
                    break;
                }
        }
        UpdateColorBase();
    }

    private bool CheckIfAnyInRadius(GameObject newGrain)
    {
        bool temp = false;
        foreach (GameObject value in list)
        {
            if (CalculateDistance(newGrain.GetComponent<Grain>().position, value.GetComponent<Grain>().position) < radiusNucleation)
            {
                temp = true;
            }
        }
        return temp;
    }

    private float CalculateDistance(Vector2 newGrain, Vector2 existingGrain)
    {
        return Vector2.Distance(newGrain, existingGrain);
    }
}

public struct Point
{
    public int x;
    public int y;
    public YPassed yPassed;
    public XPassed xPassed;
    
    public Point(int x, int y, int width, int height, string boundaryCondition)
    {
        yPassed = YPassed.NONE;
        xPassed = XPassed.NONE;
        if (boundaryCondition.Equals("Periodic"))
        {
            if (x < 0)
            {
                xPassed = XPassed.BY_LEFT;
                this.x = width+x;
            }
            else if (x >= width)
            {
                xPassed = XPassed.BY_RIGHT;
                this.x =x-width;
            }
            else
            {
                this.x = x;
            }

            if (y < 0)
            {
                yPassed = YPassed.BY_UPPER;
                this.y = height +y;
            }
            else if (y >= height)
            {
                yPassed = YPassed.BY_LOWER;
                this.y = y-height;
            }
            else
            {
                this.y = y;
            }
        }
        else
        {
            if (x < 0)
            {
                xPassed = XPassed.BY_LEFT;
                this.x = -9999;
            }
            else if (x >= width)
            {
                xPassed = XPassed.BY_RIGHT;
                this.x = -9999;
            }
            else
            {
                this.x = x;
            }

            if (y < 0)
            {
                yPassed = YPassed.BY_UPPER;
                this.y = -9999;
            }
            else if (y >= height)
            {
                yPassed = YPassed.BY_LOWER;
                this.y = -9999;
            }
            else
            {
                this.y = y;
            }
        }
    }
    public override string ToString()
    {
        return "[" + this.x + "," + this.y + "]";
    }

}

public enum YPassed
{ 
    NONE, 
    BY_UPPER,
    BY_LOWER
}

public enum XPassed
{ 
    NONE,
    BY_LEFT,
    BY_RIGHT
}

public enum DisplayMode
{ 
    STRUCTURE, 
    ENERGY
}