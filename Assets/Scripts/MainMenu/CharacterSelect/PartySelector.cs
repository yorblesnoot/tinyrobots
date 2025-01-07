using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class PartySelector : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] int startDifficulty = 1;

    [Header("Components")]
    [SerializeField] List<PartySelectionPortrait> selectionPortraits;
    [SerializeField] List<PartySelectionPortrait> partyPortraits;
    [SerializeField] BotConverter converter;
    [SerializeField] Button startButton;
    [SerializeField] PartyMemberPreview partyPreview;
    [SerializeField] Transform corePosition;
    

    List<BotCharacter> party = new();

    BotCharacter activeCharacter;
    private void Awake()
    {
        startButton.onClick.AddListener(StartGame);
        corePosition.gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
    private void Start()
    {
        
        List<BotCharacter> playables = converter.CoreLibrary.Where(core => core.Playable).ToList();
        foreach (BotCharacter character in playables) character.Initialize();
        playables.PassDataToUI(selectionPortraits, PopulatePortrait);
        SetActiveCharacter(playables[0]);
    }

    void PopulatePortrait(BotCharacter character, PartySelectionPortrait portrait)
    {
        portrait.Become(character, () => SelectCharacter(character), () => FastSelectCharacter(character));
    }

    void FastSelectCharacter(BotCharacter character)
    {
        AddCharacterToParty(character);
        SetActiveCharacter(character);
    }

    void SelectCharacter(BotCharacter character)
    {
        if(activeCharacter == character)
        {
            AddCharacterToParty(character);
        }
        else
        {
            
            SetActiveCharacter(character);
        }
    }

    void AddCharacterToParty(BotCharacter character)
    {
        if (partyPortraits.Count == 0 || party.Contains(character)) return;
        PartySelectionPortrait portrait = partyPortraits[0];
        portrait.Become(character, () => RemoveCharacterFromParty(character, portrait));
        partyPortraits.RemoveAt(0);
        party.Add(character);
    }

    void RemoveCharacterFromParty(BotCharacter character, PartySelectionPortrait portait)
    {
        party.Remove(character);
        portait.Clear();
        partyPortraits.Insert(0, portait);
    }

    void SetActiveCharacter(BotCharacter character)
    {
        if(activeCharacter != null && activeCharacter != character) activeCharacter.ModdedCore.Sample.SetActive(false);
        activeCharacter = character;
        GameObject sample = character.ModdedCore.Sample;
        sample.SetActive(true);
        sample.transform.SetParent(corePosition);
        sample.transform.localPosition = Vector3.zero;
        partyPreview.Become(character);
    }

    void StartGame()
    {
        converter.Initialize();
        SceneGlobals.PlayerData.CoreInventory = party;
        foreach (var core in SceneGlobals.PlayerData.CoreInventory)
        {
            core.HealthRatio = 1;
            if(SceneGlobals.PlayerData.DevMode)
            {
                core.Initialize();
                core.Bot = new(core.ModdedCore);
            }
            else core.Bot = SceneGlobals.PlayerData.BotConverter.StringToBot(core.StarterRecord.Record);
        }
        SceneGlobals.SceneRelay.GenerateNavMap = true;
        SceneGlobals.PlayerData.PartInventory = new();
        SceneGlobals.PlayerData.Difficulty = startDifficulty;
        SceneGlobals.PlayerData.MapData = new();
        SceneGlobals.PlayerData.ShopData = new();
        SceneLoader.Load(SceneType.NAVIGATION);
    }
}
