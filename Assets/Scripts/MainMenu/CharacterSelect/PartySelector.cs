using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
    

    List<BotCharacter> party = new();

    BotCharacter activeCharacter;
    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
        converter.CoreLibrary.Where(core => core.Playable).ToList().PassDataToUI(selectionPortraits, PopulatePortrait);
    }

    void PopulatePortrait(BotCharacter character, PartySelectionPortrait portrait)
    {
        portrait.Become(character, () => SelectCharacter(character));
    }

    void SelectCharacter(BotCharacter character)
    {
        if(activeCharacter == character)
        {
            AddCharacterToParty(character);
        }
        else
        {
            activeCharacter = character;
            ShowCharacterInfo(character);
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

    void ShowCharacterInfo(BotCharacter character)
    {

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
        SceneLoader.Load(SceneType.NAVIGATION);
    }
}