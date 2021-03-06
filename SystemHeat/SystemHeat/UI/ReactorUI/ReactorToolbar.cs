﻿using KSP.UI;
using KSP.UI.Screens;
using System.Collections.Generic;
using UnityEngine;

namespace SystemHeat.UI
{
  [KSPAddon(KSPAddon.Startup.Flight, false)]
  public class ReactorToolbar : MonoBehaviour
  {
    public static ReactorToolbar Instance { get; private set; }
    // Control Vars
    protected static bool showWindow = false;

    // Vessel-related variables
    private Vessel activeVessel;
    private int partCount = 0;


    // Panel
    protected ReactorPanel reactorPanel;

    // Stock toolbar button
    protected string toolbarUIIconURLOff = "SystemHeat/UI/toolbar_reactor_off";
    protected string toolbarUIIconURLOn = "SystemHeat/UI/toolbar_reactor_on";
    protected static ApplicationLauncherButton stockToolbarButton = null;

    protected virtual void Awake()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[ReactorToolbar]: Initializing toolbar");

      GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
      GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);

      GameEvents.onGUIApplicationLauncherUnreadifying.Add(new EventData<GameScenes>.OnEvent(OnGUIAppLauncherUnreadifying));
      GameEvents.onVesselChange.Add(new EventData<Vessel>.OnEvent(OnVesselChanged));

      Instance = this;
    }

    public void Start()
    {
      if (ApplicationLauncher.Ready)
        OnGUIAppLauncherReady();


    }
    protected void CreateToolbarPanel()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[ReactorToolbar]: Creating toolbar panel");
      GameObject newUIPanel = (GameObject)Instantiate(SystemHeatUILoader.ReactorToolbarPanelPrefab, Vector3.zero, Quaternion.identity);
      newUIPanel.transform.SetParent(UIMasterController.Instance.appCanvas.transform);
      newUIPanel.transform.localPosition = Vector3.zero;
      reactorPanel = newUIPanel.AddComponent<ReactorPanel>();
      reactorPanel.SetVisible(false);
    }
    protected void DestroyToolbarPanel()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[ReactorToolbar]: Destroying toolbar panel");
      if (reactorPanel != null)
      {
        Destroy(reactorPanel.gameObject);
      }
    }

    public void ToggleAppLauncher()
    {
      showWindow = !showWindow;
      reactorPanel.SetVisible(showWindow);

    }
    void Update()
    {
      if (showWindow && reactorPanel != null)
      {

        if (HighLogic.LoadedSceneIsFlight)
        {

          reactorPanel.rect.position = stockToolbarButton.GetAnchorUL() - new Vector3(reactorPanel.rect.rect.width, reactorPanel.rect.rect.height, 0f);
        }
        if (HighLogic.LoadedSceneIsEditor)
        {
          if (stockToolbarButton != null)
            reactorPanel.rect.position = stockToolbarButton.GetAnchorUL();

        }
      }
    }
    public void OnVesselChanged(Vessel v)
    {
      // Refresh reactors
      ClearReactors();
      FindReactors(v);

    }

    public void ClearReactors()
    {
      if (reactorPanel != null)
        reactorPanel.ClearReactors();
    }
    public void FindReactors(Vessel ves)
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log($"[ReactorToolbar]: Detecting reactors on {ves}");

      activeVessel = ves;
      partCount = ves.parts.Count;


      List<PartModule> unsortedReactorList = new List<PartModule>();
      // Get all parts
      List<Part> allParts = ves.parts;
      for (int i = 0; i < allParts.Count; i++)
      {
        for (int j = 0; j < allParts[i].Modules.Count; j++)
        {
          if (allParts[i].Modules[j].moduleName == "ModuleSystemHeatFissionReactor" ||
            allParts[i].Modules[j].moduleName == "ModuleSystemHeatFissionEngine" ||
            allParts[i].Modules[j].moduleName == "ModuleFusionEngine" ||
            allParts[i].Modules[j].moduleName == "FusionReactor")
          {
            unsortedReactorList.Add(allParts[i].Modules[j]);
          }
        }
      }
      if (SystemHeatSettings.DebugUI)
        Utils.Log($"[ReactorToolbar]: found {unsortedReactorList.Count} reactors");

      if (reactorPanel)
        foreach (PartModule reactor in unsortedReactorList)
        {
          reactorPanel.AddReactor(reactor);
        }
    }

    #region Stock Toolbar Methods
    public void OnDestroy()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[ReactorToolbar]: OnDestroy Fired");
      // Remove the stock toolbar button
      GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
      GameEvents.onVesselChange.Remove(OnVesselChanged);
      if (stockToolbarButton != null)
      {
        ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
      }
    }

    protected void OnToolbarButtonToggle()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[ReactorToolbar]: Toolbar Button Toggled");

      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showWindow ? toolbarUIIconURLOn : toolbarUIIconURLOff, false));
      ToggleAppLauncher();
    }


    protected void OnGUIAppLauncherReady()
    {
      showWindow = false;
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[UIReactorToolbar App Launcher Ready");
      if (ApplicationLauncher.Ready && stockToolbarButton == null)
      {
        stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
            OnToolbarButtonToggle,
            OnToolbarButtonToggle,
            DummyVoid,
            DummyVoid,
            DummyVoid,
            DummyVoid,
            ApplicationLauncher.AppScenes.FLIGHT,
            (Texture)GameDatabase.Instance.GetTexture(toolbarUIIconURLOff, false));
      }
      CreateToolbarPanel();

      FindReactors(FlightGlobals.ActiveVessel);

    }

    protected void OnGUIAppLauncherDestroyed()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[UIReactorToolbar App Launcher Destroyed");
      if (stockToolbarButton != null)
      {
        ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
        stockToolbarButton = null;
      }
      DestroyToolbarPanel();
    }


    protected void OnGUIAppLauncherUnreadifying(GameScenes scene)
    {

      if (SystemHeatSettings.DebugUI)
        Utils.Log("[ReactorToolbar]: App Launcher Unready");

      DestroyToolbarPanel();
    }

    protected void onAppLaunchToggleOff()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[ReactorToolbar]: App Launcher Toggle Off");
      stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(toolbarUIIconURLOff, false));
    }

    protected void DummyVoid() { }

    public void ResetAppLauncher()
    {
      if (SystemHeatSettings.DebugUI)
        Utils.Log("[ReactorToolbar]: Reset App Launcher");
      //FindData();
      if (stockToolbarButton == null)
      {
        stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
            OnToolbarButtonToggle,
            OnToolbarButtonToggle,
            DummyVoid,
            DummyVoid,
            DummyVoid,
            DummyVoid,
            ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT,
            (Texture)GameDatabase.Instance.GetTexture(toolbarUIIconURLOff, false));
      }

    }
    #endregion
  }
}
