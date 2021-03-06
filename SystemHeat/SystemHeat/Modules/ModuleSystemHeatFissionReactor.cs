using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SystemHeat
{
  /// <summary>
  /// The connection between a stock ModuleResourceHarvester and the SystemHeat system
  /// </summary>
  public class ModuleSystemHeatFissionReactor : PartModule
  {
    // This should be unique on the part
    [KSPField(isPersistant = false)]
    public string moduleID;

    // This should correspond to the related ModuleSystemHeat
    [KSPField(isPersistant = false)]
    public string systemHeatModuleID;

    // --- General -----
    [KSPField(isPersistant = true)]
    public bool Enabled = false;

    // Current reactor power setting (0-100, tweakable)
    [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Power Setting", groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
    public float CurrentReactorThrottle = 100f;

    // Current power generation
    [KSPField(isPersistant = true)]
    public float CurrentThrottle = 0f;

    [KSPField(isPersistant = false)]
    public float MinimumThrottle = 25f;

    // Current power generation in %/s
    [KSPField(isPersistant = false)]
    public float ThrottleIncreaseRate = 1f;

    // -- POWER

    [KSPField(isPersistant = false)]
    public bool GeneratesElectricity = true;
    // Amount of power generated by the reactor, scaled by throttle setting
    [KSPField(isPersistant = false)]
    public FloatCurve ElectricalGeneration = new FloatCurve();

    // Current electricity generation
    [KSPField(isPersistant = true)]
    public float CurrentElectricalGeneration = 0f;

    // Reactor Status string
    [KSPField(isPersistant = false, guiActive = true, guiName = "Generation", groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title")]
    public string GeneratorStatus;

    // Name of the fuel
    [KSPField(isPersistant = false)]
    public string FuelName = "EnrichedUranium";

    // --- Thermals -----
    // Heat generation at full power
    [KSPField(isPersistant = false)]
    public float HeatGeneration;

    // Heat generation at full power
    [KSPField(isPersistant = false)]
    public float CurrentHeatGeneration;


    // Nominal reactor temperature (where the reactor should live)
    [KSPField(isPersistant = false)]
    public float NominalTemperature = 900f;

    // Critical reactor temperature (reactor function reduced)
    [KSPField(isPersistant = false)]
    public float CriticalTemperature = 1400f;

    // Maximum reactor temperature (core damage after this)
    [KSPField(isPersistant = false)]
    public float MaximumTemperature = 2000f;

    // Safety override 
    [KSPField(isPersistant = true, guiActive = true, guiName = "Auto-Shutdown Temp", groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title"), UI_FloatRange(minValue = 700f, maxValue = 6000f, stepIncrement = 100f)]
    public float CurrentSafetyOverride = 1000f;

    [KSPField(isPersistant = true)]
    public bool FirstLoad = true;

    [KSPField(isPersistant = true)]
    public double LastUpdateTime = -1d;

    // REPAIR VARIABLES
    // integrity of the core
    [KSPField(isPersistant = true)]
    public float CoreIntegrity = 100f;

    // Rate the core is damaged, in % per S per K
    [KSPField(isPersistant = false)]
    public float CoreDamageRate = 0.005f;

    // Engineer level to repair the core
    [KSPField(isPersistant = false)]
    public int EngineerLevelForRepair = 5;

    [KSPField(isPersistant = false)]
    public float MaxRepairPercent = 75;

    [KSPField(isPersistant = false)]
    public float MinRepairPercent = 10;

    [KSPField(isPersistant = false)]
    public float MaxTempForRepair = 325;

    // This should be unique on the part
    [KSPField(isPersistant = false)]
    public string uiGroupName = "fissionreactor";
    // This should be unique on the part
    [KSPField(isPersistant = false)]
    public string uiGroupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title";

    /// UI FIELDS
    /// --------------------
    // Reactor Status string
    [KSPField(isPersistant = false, guiActive = true, guiName = "Reactor Power", groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title")]
    public string ReactorOutput;

    // integrity of the core
    [KSPField(isPersistant = false, guiActive = true, guiName = "Core Temperature", groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title")]
    public string CoreTemp;

    // integrity of the core
    [KSPField(isPersistant = false, guiActive = true, guiName = "Core Health", groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title")]
    public string CoreStatus;

    // Fuel Status string
    [KSPField(isPersistant = false, guiActive = true, guiName = "Core Life", groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title")]
    public string FuelStatus;

    /// KSPEVENTS
    /// ----------------------
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Event_Enable_Title", active = true, groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public void EnableReactor()
    {
      ReactorActivated();
    }
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Event_Disable_Title", active = false, groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public void DisableReactor()
    {
      ReactorDeactivated();
    }
    /// Toggle control panel
    [KSPEvent(guiActive = false, guiName = "Toggle Reactor Control", active = true, groupName = "fissionreactor", groupDisplayName = "#LOC_SystemHeat_ModuleSystemHeatFissionReactor_UIGroup_Title")]
    public void ShowReactorControl()
    {
      /// TODO: UI DAMN
      //ReactorUI.ToggleReactorWindow();
    }
    // Try to fix the reactor
    [KSPEvent(externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.5f, guiName = "Repair Reactor")]
    public void RepairReactor()
    {
      if (TryRepairReactor())
      {
        DoReactorRepair();
      }
    }
    /// KSPACTIONS
    /// ----------------------

    [KSPAction("Enable Reactor")]
    public void EnableAction(KSPActionParam param)
    {
      EnableReactor();
    }

    [KSPAction("Disable Reactor")]
    public void DisableAction(KSPActionParam param)
    {
      DisableReactor();
    }

    [KSPAction("Toggle Reactor")]
    public void ToggleAction(KSPActionParam param)
    {
      if (!Enabled) EnableReactor();
      else DisableReactor();
    }

    [KSPAction("Toggle Reactor Panel")]
    public void TogglePanelAction(KSPActionParam param)
    {
      ShowReactorControl();
    }

    protected ModuleSystemHeat heatModule;
    protected List<ResourceRatio> inputs;
    protected List<ResourceRatio> outputs;

    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_ModuleName");
    }

    public override string GetInfo()
    {
      double baseRate = 0d;
      for (int i = 0; i < inputs.Count; i++)
      {
        if (inputs[i].ResourceName == FuelName)
          baseRate = inputs[i].Ratio;
      }
      return
          Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_PartInfo",
          ElectricalGeneration.Evaluate(100f).ToString("F0"),
          FindTimeRemaining(this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(FuelName).id).amount, baseRate),
          HeatGeneration.ToString("F0"),
          NominalTemperature.ToString("F0"),
          NominalTemperature.ToString("F0"),
          CriticalTemperature.ToString("F0"),
          MaximumTemperature.ToString("F0"),
          ThrottleIncreaseRate.ToString("F0"),
          MinimumThrottle.ToString("F0"));

    }


    public virtual void ReactorActivated()
    {
      Enabled = true;
    }

    public virtual void ReactorDeactivated()
    {
      Enabled = false;
    }

    public virtual void Start()
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (inputs == null || inputs.Count == 0)
        {
          ConfigNode node = GameDatabase.Instance.GetConfigs("PART").
              Single(c => part.partInfo.name == c.name).config.
              GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName);
          OnLoad(node);
        }

        heatModule = this.GetComponents<ModuleSystemHeat>().ToList().Find(x => x.moduleID == systemHeatModuleID);
        if (heatModule == null)
          heatModule.GetComponent<ModuleSystemHeat>();

        var range = (UI_FloatRange)this.Fields["CurrentSafetyOverride"].uiControlEditor;
        range.minValue = 0f;
        range.maxValue = MaximumTemperature;

        range = (UI_FloatRange)this.Fields["CurrentSafetyOverride"].uiControlFlight;
        range.minValue = 0f;
        range.maxValue = MaximumTemperature;

        foreach (BaseField field in this.Fields)
        {
          if (!string.IsNullOrEmpty(field.group.name)) continue;

          if (field.group.name == uiGroupName)
            field.group.displayName = Localizer.Format(uiGroupDisplayName);
        }

        foreach (BaseEvent baseEvent in this.Events)
        {
          if (!string.IsNullOrEmpty(baseEvent.group.name)) continue;

          if (baseEvent.group.name == uiGroupName)
            baseEvent.group.displayName = Localizer.Format(uiGroupDisplayName);
        }

        if (!GeneratesElectricity)
        {
          Fields["GeneratorStatus"].guiActive = false;
        }
        Actions["TogglePanelAction"].guiName = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Action_TogglePanelAction");
      }

      if (FirstLoad)
      {
        this.CurrentSafetyOverride = this.CriticalTemperature;
        FirstLoad = false;
      }
      if (HighLogic.LoadedSceneIsFlight)
      {
        GameEvents.OnVesselRollout.Add(new EventData<ShipConstruct>.OnEvent(OnVesselRollout));
        DoCatchup();
      }

    }
    void OnDestroy()
    {
      // Clean up events when the item is destroyed
      GameEvents.OnVesselRollout.Remove(OnVesselRollout);
    }
    /// <summary>
    /// 
    /// </summary>
    protected void OnVesselRollout(ShipConstruct node)
    {
      CoreIntegrity = 100f;
      CurrentHeatGeneration = 0f;
    }

    public void DoCatchup()
    {
      if (part.vessel.missionTime > 0.0)
      {
        if (Enabled)
        {
          double elapsedTime = Planetarium.GetUniversalTime() - LastUpdateTime;
          if (elapsedTime > 0d)
          {
            Utils.Log($"[SystemHeatFissionReactor] Catching up {elapsedTime} s of time on load");
            float fuelThrottle = CurrentReactorThrottle / 100f;

            foreach (ResourceRatio ratio in inputs)
            {
              Utils.Log($"[SystemHeatFissionReactor] Consuming {fuelThrottle * ratio.Ratio * elapsedTime} u of {ratio.ResourceName} on load");
              double amt = this.part.RequestResource(ratio.ResourceName, fuelThrottle * ratio.Ratio * elapsedTime, ratio.FlowMode);

            }

          }
        }
      }
    }

    public override void OnLoad(ConfigNode node)
    {
      base.OnLoad(node);


      /// Load resource nodes
      ConfigNode[] inNodes = node.GetNodes("INPUT_RESOURCE");

      inputs = new List<ResourceRatio>();
      for (int i = 0; i < inNodes.Length; i++)
      {
        ResourceRatio p = new ResourceRatio();
        p.Load(inNodes[i]);
        inputs.Add(p);
      }
      ConfigNode[] outNodes = node.GetNodes("OUTPUT_RESOURCE");

      outputs = new List<ResourceRatio>();
      for (int i = 0; i < outNodes.Length; i++)
      {
        ResourceRatio p = new ResourceRatio();
        p.Load(outNodes[i]);
        outputs.Add(p);
      }

    }

    public void Update()
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (Events["EnableReactor"].active == Enabled || Events["DisableReactor"].active != Enabled)
        {
          Events["DisableReactor"].active = Enabled;
          Events["EnableReactor"].active = !Enabled;
        }
      }
    }

    public virtual void FixedUpdate()
    {
      if (HighLogic.LoadedSceneIsEditor)
      {
        HandleHeatGenerationEditor();
        if (GeneratesElectricity)
          CurrentElectricalGeneration = ElectricalGeneration.Evaluate(CurrentThrottle);

      }
      if (HighLogic.LoadedSceneIsFlight)
      {
        if (part.vessel.missionTime > 0.0)
        {
          LastUpdateTime = Planetarium.GetUniversalTime();
        }
        // Update reactor core integrity readout
        if (CoreIntegrity > 0)
          CoreStatus = String.Format("{0:F2} %", CoreIntegrity);
        else
          CoreStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_CoreStatus_Meltdown");

        HandleCoreDamage();
        HandleThrottle();
        HandleHeatGeneration();

        // IF REACTOR ON
        // =============
        if (Enabled)
        {
          HandleResourceActivities(TimeWarp.fixedDeltaTime);
          if (heatModule.currentLoopTemperature > CurrentSafetyOverride)
          {
            ReactorDeactivated();
          }
        }
        // IF REACTOR OFF
        // =============
        else
        {
          // Update UI
          if (CoreIntegrity <= 0f)
          {
            FuelStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_FuelStatus_Meltdown");
            ReactorOutput = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_ReactorOutput_Meltdown");
          }
          else
          {
            FuelStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_FuelStatus_Offline");
            ReactorOutput = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_ReactorOutput_Offline");
          }
        }
      }
    }

    protected virtual void HandleThrottle()
    {
      if (!Enabled)
      {
        CurrentThrottle = Mathf.MoveTowards(CurrentThrottle, 0f, TimeWarp.fixedDeltaTime * ThrottleIncreaseRate);
      }
      else
      {
        CurrentThrottle = Mathf.MoveTowards(CurrentThrottle, CurrentReactorThrottle, TimeWarp.fixedDeltaTime * ThrottleIncreaseRate);
      }
      CoreTemp = String.Format("{0:F1}/{1:F1} {2}", heatModule.LoopTemperature, NominalTemperature, Localizer.Format("#LOC_SystemHeat_Units_K"));
    }

    protected virtual float CalculateHeatGeneration()
    {
      return (CurrentThrottle / 100f * HeatGeneration) * CoreIntegrity / 100f;
    }
    protected virtual void HandleHeatGeneration()
    {
      // Determine heat to be generated
      CurrentHeatGeneration = CalculateHeatGeneration();
      heatModule.AddFlux(moduleID, NominalTemperature, CurrentHeatGeneration);

      if (CoreIntegrity <= 0f)
      {
        FuelStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_FuelStatus_Meltdown");
        ReactorOutput = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_ReactorOutput_Meltdown");
      }
      else
      {
        ReactorOutput = String.Format("{0:F1} {1}", CurrentHeatGeneration, Localizer.Format("#LOC_SystemHeat_Units_kW"));
      }
    }
    private void HandleHeatGenerationEditor()
    {
      CurrentHeatGeneration = (CurrentReactorThrottle / 100f * HeatGeneration);
      heatModule.AddFlux(moduleID, NominalTemperature, CurrentHeatGeneration);
    }


    // track and set core damage
    private void HandleCoreDamage()
    {
      // Update reactor damage
      float critExceedance = heatModule.LoopTemperature - CriticalTemperature;

      // If overheated too much, damage the core
      if (critExceedance > 0f && TimeWarp.CurrentRate < 100f)
      {
        // core is damaged by Rate * temp exceedance * time
        CoreIntegrity = Mathf.MoveTowards(CoreIntegrity, 0f, CoreDamageRate * critExceedance * TimeWarp.fixedDeltaTime);
      }

      // Calculate percent exceedance of nominal temp
      float tempNetScale = 1f - Mathf.Clamp01((heatModule.LoopTemperature - NominalTemperature) / (MaximumTemperature - NominalTemperature));
    }

    protected virtual float CalculateGoalThrottle(float timeStep)
    {
      double shipEC = 0d;
      double shipMaxEC = 0d;
      // Determine need for power
      part.GetConnectedResourceTotals(PartResourceLibrary.ElectricityHashcode, out shipEC, out shipMaxEC, true);

      float maxGeneration = ElectricalGeneration.Evaluate(100f) * CoreIntegrity / 100f;
      float minGeneration = ElectricalGeneration.Evaluate(MinimumThrottle) * timeStep;
      float idealGeneration = Mathf.Min(maxGeneration * timeStep, (float)(shipMaxEC - shipEC));
      float powerToGenerate = Mathf.Max(minGeneration, idealGeneration);

      return (powerToGenerate / timeStep) / maxGeneration * 100f;
    }

    private void HandleResourceActivities(float timeStep)
    {

      CurrentReactorThrottle = CalculateGoalThrottle(timeStep);

      double burnRate = 0d;
      float fuelThrottle = CurrentReactorThrottle / 100f;

      bool fuelCheckPassed = true;
      foreach (ResourceRatio ratio in inputs)
      {
        double amt = this.part.RequestResource(ratio.ResourceName, fuelThrottle * ratio.Ratio * timeStep, ratio.FlowMode);
        if (amt < 0.0000000000001)
        {
          ReactorDeactivated();
          fuelCheckPassed = false;
        }
        if (ratio.ResourceName == FuelName)
          burnRate = ratio.Ratio;
      }

      if (GeneratesElectricity)
      {
        if (HighLogic.LoadedSceneIsEditor)
          CurrentElectricalGeneration = ElectricalGeneration.Evaluate(CurrentReactorThrottle);
        if (fuelCheckPassed)
        {
          foreach (ResourceRatio ratio in outputs)
          {
            double amt = this.part.RequestResource(ratio.ResourceName, -fuelThrottle * ratio.Ratio * timeStep, ratio.FlowMode);
          }

          CurrentElectricalGeneration = ElectricalGeneration.Evaluate(CurrentThrottle);
          this.part.RequestResource(PartResourceLibrary.ElectricityHashcode, -CurrentElectricalGeneration * timeStep, ResourceFlowMode.ALL_VESSEL);

          GeneratorStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_GeneratorStatus_Normal", CurrentElectricalGeneration.ToString("F1"));
        }
        else
        {
          GeneratorStatus = Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_GeneratorStatus_Offline");
        }
      }

      // Find the time remaining at current rate
      FuelStatus = FindTimeRemaining(
        this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(FuelName).id).amount,
        burnRate);

    }


    #region Repair
    public bool TryRepairReactor()
    {
      if (CoreIntegrity <= MinRepairPercent)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Message_Repair_CoreTooDamaged"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      if (!ModuleUtils.CheckEVAEngineerLevel(EngineerLevelForRepair))
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Message_Repair_CoreTooDamaged", EngineerLevelForRepair.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      if (Enabled)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Message_Repair_NotWhileRunning"),
            5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      if (heatModule.LoopTemperature > MaxTempForRepair)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Message_Repair_CoreTooHot", MaxTempForRepair.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      if (CoreIntegrity >= MaxRepairPercent)
      {
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Message_Repair_CoreAlreadyRepaired", MaxRepairPercent.ToString("F0")),
            5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      return true;
    }

    // Repair the reactor to max Repair percent
    public void DoReactorRepair()
    {
      this.CoreIntegrity = MaxRepairPercent;
      ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Message_Repair_RepairSuccess",
        MaxRepairPercent.ToString("F0")), 5.0f, ScreenMessageStyle.UPPER_CENTER));
    }

    #endregion

    #region Refuelling
    // Finds time remaining at specified fuel burn rates
    public string FindTimeRemaining(double amount, double rate)
    {
      if (rate < 0.0000001)
      {
        return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_FuelStatus_VeryLong");
      }
      double remaining = amount / rate;
      if (remaining >= 0)
      {
        return ModuleUtils.FormatTimeString(remaining);
      }
      {
        return Localizer.Format("#LOC_SystemHeat_ModuleSystemHeatFissionReactor_Field_FuelStatus_Exhausted");
      }
    }
    #endregion
  }


}

