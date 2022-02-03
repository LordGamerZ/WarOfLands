using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

public class UIControl : MonoBehaviour
{
    public static UIControl Instance;

    public EventSystem EV;
    public GraphicRaycaster GR;
    public Transform ProfileContainer;

    public GameObject MainScreen;
    public GameObject LoseScreen;
    public GameObject WinScreen;
    public GameObject OptionsMenu;

    public GameObject PrefabProfileIcon;

    public Profile SetPathProfile;
    public Profile RemoveForestProfile;
    public ProfileToolTip ToolTip;

    public TextMeshProUGUI WoodText;
    public TextMeshProUGUI OreText;
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI TimerDisplay;
    public TextMeshProUGUI TurnDisplay;

    public Sprite SetPathIcon;
    public Sprite RemoveForestIcon;
    public Sprite ArrowSprite;
    public Sprite HourGlassSprite;
    public Image ChangeTurnImage;

    public float Wood;
    public float Stone;
    public float Gold;

    public float TurnTime;
    public float TurnTimer;

    public int NumTurns;

    public bool IsBuilding;
    public bool IsRecruiting;
    public bool OptionsOpen;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;

            IsBuilding = false;
            IsRecruiting = false;
            OptionsOpen = false;

            MainScreen.SetActive(true);
            LoseScreen.SetActive(false);
            OptionsMenu.SetActive(false);
            WinScreen.SetActive(false);

            gameObject.SetActive(false);

            ToolTip.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        ChangerResources(200, 200, 0);
    }

    private void Update()
    {
        if (PlayerInteraction.Instance.IsTurn)
        {
            TurnTimer += Time.deltaTime;
            TimerDisplay.text = "Time left " + (TurnTime - Mathf.RoundToInt(TurnTimer)).ToString();

            if(TurnTime < TurnTimer)
            {
                NextTurn();
            }
        }
        else
        {
            TurnTimer = 0;
            TimerDisplay.text = "Waiting...";
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptions();
        }

        PointerEventData PointerEventData = new(EV)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> hits = new();
        GR.Raycast(PointerEventData, hits);

        bool isProfile = false;

        for (int i = 0; i < hits.Count; i++)
        {
            ProfileIcon profileIcon = hits[i].gameObject.GetComponent<ProfileIcon>();
            if (profileIcon)
            {
                isProfile = true;
                string additional = "";
                if(profileIcon.P as BuildingProfile)
                {
                    additional = "Built on " + (profileIcon.P as BuildingProfile).BiomeType.ToString();
                }

                ToolTip.SetTexts(profileIcon.P.name, profileIcon.P.Description, profileIcon.P.ProductionTime, profileIcon.P.GoldCost, profileIcon.P.StoneCost, profileIcon.P.WoodCost, additional);
                break;
            }
        }

        if(!isProfile)
        {
            ToolTip.gameObject.SetActive(false);
        }
        else
        {
            ToolTip.gameObject.SetActive(true);
            ToolTip.transform.position = Input.mousePosition;
        }
    }

    public void DisplayLose()
    {
        CloseProfileMenu();
        MainScreen.SetActive(false);

        if (OptionsOpen)
        {
            ToggleOptions();
        }

        LoseScreen.SetActive(true);
    }

    public void DisplayWin()
    {
        CloseProfileMenu();
        MainScreen.SetActive(false);

        if (OptionsOpen)
        {
            ToggleOptions();
        }

        WinScreen.SetActive(true);
    }

    public void Leave()
    {
     //   Client.Instance.LobbyNum = 0;
      //  Client.Instance.LoadScene("CreateJoinLobby");
    }

    public void ToggleOptions()
    {
        OptionsOpen = !OptionsOpen;
        OptionsMenu.SetActive(OptionsOpen);
    }

    public void Quit()
    {
   //     ClientSend.LeaveLobby();
      //  Client.Instance.LobbyNum = 0;
      //  Client.Instance.LoadScene("CreateJoinLobby");
    }

    public void ChangerResources(float wood, float stone, float gold)
    {
        Wood = wood;
        Stone = stone;
        Gold = gold;

        WoodText.text = "Wood " + Wood.ToString();
        OreText.text = "Stone " + Stone.ToString();
        GoldText.text = "Gold " + Gold.ToString();
    }

    public void UpdateResources(float wood, float stone, float gold)
    {
        Wood += wood;
        Stone += stone;
        Gold += gold;

        WoodText.text = "Wood " + Wood.ToString();
        OreText.text = "Stone " + Stone.ToString();
        GoldText.text = "Gold " + Gold.ToString();
    }

    public void ShowRecruitMenu()
    {
        if (IsRecruiting)
        {
            CloseProfileMenu();
        }

        BasicFactoryCommands factory = PlayerInteraction.Instance.Selected.GetComponent<BasicFactoryCommands>();

        if (factory)
        {
            IsRecruiting = true;

            for (int i = 0; i < factory.Profiles.Count; i++)
            {
                ProfileIcon profileIcon = Instantiate(PrefabProfileIcon, ProfileContainer).GetComponent<ProfileIcon>();

                profileIcon.GetComponent<Image>().sprite = factory.Profiles[i].Icon;
                profileIcon.P = factory.Profiles[i];
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(ProfileContainer.GetComponent<RectTransform>());
        }
    }

    public void ShowBuildMenu()
    {
        if (IsBuilding)
        {
            CloseProfileMenu();
        }

        BuilderCommands builder = PlayerInteraction.Instance.Selected.GetComponent<BuilderCommands>();

        if (builder)
        {
            IsBuilding = true;

            for (int i = 0; i < builder.Profiles.Count; i++)
            {
                ProfileIcon profileIcon = Instantiate(PrefabProfileIcon, ProfileContainer).GetComponent<ProfileIcon>();

                profileIcon.GetComponent<Image>().sprite = builder.Profiles[i].Icon;
                profileIcon.P = builder.Profiles[i];
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(ProfileContainer.GetComponent<RectTransform>());
    }

    public void CloseProfileMenu()
    {
        if (IsRecruiting || IsBuilding)
        {
            IsRecruiting = false;
            IsBuilding = false;

            for (int i = ProfileContainer.childCount - 1; i > -1; i--)
            {
                Destroy(ProfileContainer.GetChild(i).gameObject);
            }
        }
    }

    //Tells the next player that its their turn
    public void NextTurn()
    {
        if (PlayerInteraction.Instance.IsTurn)
        {
            bool foundMe = false;
            bool done = false;

            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < GameManager.Instance.PlayerIDs.Length; i++)
                {
                    if (foundMe)
                    {
                        done = true;
                        GameManager.Instance.gameObject.GetPhotonView().RPC("MyTurn", RpcTarget.All, GameManager.Instance.PlayerIDs[i]);
                        break;
                    }
                    else if (GameManager.Instance.PlayerIDs[i] == PhotonNetwork.LocalPlayer.ActorNumber)
                    {
                        foundMe = true;
                    }
                }

                if(done)
                {
                    break;
                }
            }
        }
    }

    //Changes sprite based on whether it's the current players turn    
    public void ChangeArrowSprite()
    {
        TurnTimer = 0; 

        if (PlayerInteraction.Instance.IsTurn)
        {
            ChangeTurnImage.sprite = ArrowSprite;
        }
        else
        {
            ChangeTurnImage.sprite = HourGlassSprite;
        }
    }

    //Update turn counter in UI
    public void IncrementTurns()
    {
        NumTurns += 1;
        TurnDisplay.text = "Turn " + NumTurns.ToString();

        PlayerInteraction.Instance.NextTurn();
    }

    //If a transaction, e.g. attempt to start a building or unit production can be afforded 
    public bool CanAfford(float wood, float stone, float gold)
    {
        if(Wood + wood >= 0)
        {
            if(Stone + stone >= 0)
            {
                if(Gold + gold >= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
