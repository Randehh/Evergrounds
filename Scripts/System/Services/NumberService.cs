using System;
using System.Collections.Generic;
using System.Linq;

public class NumberService : IService
{
    public Action<NumberType, float> OnNumberUpdated { get; set; } = delegate { };

    private Dictionary<NumberType, float> baseNumbers = new() {
        [NumberType.MOVE_SPEED] = 50
    };
    private Dictionary<NumberType, float> calculatedValues = new();
    private Dictionary<NumberType, List<NumberMod>> numberMods = new();

    public void OnInit()
    {
        foreach (NumberType numberType in Enum.GetValues(typeof(NumberType))) {
            baseNumbers.TryAdd(numberType, 1);
            numberMods[numberType] = new();
        }
    }

    public void OnReady()
    {

    }

    public void OnDestroy()
    {

    }

    public void AddMods(params  NumberMod[] mods)
    {
        foreach (NumberMod mod in mods) {
            AddMod(mod.id, mod.numberType, mod.modType, mod.modValue);
        }
    }

    public void AddMod(string id, NumberType type, NumberModType numberModType, float value)
    {
        numberMods[type].Add(new NumberMod() {
            id = id,
            modType = numberModType,
            modValue = value
        });

        UpdateValue(type);
    }

    public void RemoveMods(params NumberMod[] mods)
    {
        foreach (NumberMod mod in mods) {
            RemoveMod(mod.id, mod.numberType);
        }
    }

    public void RemoveMod(string id, NumberType type)
    {
        NumberMod mod = numberMods[type].Find(mod => mod.id == id);
        if(mod == null) {
            return;
        }

        numberMods[type].Remove(mod);

        UpdateValue(type);
    }

    private void UpdateValue(NumberType type)
    {
        float value = baseNumbers[type];
        List<NumberMod> modList = numberMods[type].OrderBy(mod => mod.modType).ToList();

        float additiveTotal = 1;
        foreach (NumberMod mod in modList) {
            switch (mod.modType) {
                case NumberModType.ADD:
                    value += mod.modValue;
                    break;

                case NumberModType.SUBTRACT:
                    value -= mod.modValue;
                    break;

                case NumberModType.MULTIPLY:
                    value *= mod.modValue;
                    break;

                case NumberModType.MULTIPLY_ADDITIVE:
                    additiveTotal += mod.modValue;
                    break;
            }
        }

        value *= additiveTotal;

        calculatedValues[type] = value;

        OnNumberUpdated.Invoke(type, value);
    }

    public float GetCalculatedValue(NumberType type)
    {
        if (!calculatedValues.ContainsKey(type)) {
            UpdateValue(type);
        }
        return calculatedValues[type];
    }
}

public enum NumberType
{
    MOVE_SPEED,
    ATTACK_SPEED,
    CHARISMA,
}

public enum NumberModType
{
    ADD,
    SUBTRACT,
    MULTIPLY,
    MULTIPLY_ADDITIVE,
}