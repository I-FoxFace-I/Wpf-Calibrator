//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Calibrator.WpfApplication.Models;

//namespace Calibrator.WpfApplication.Services;

//// Mock repositories for demo purposes
//public interface IControllerRepository
//{
//    Task<List<Controller>> GetAllWithNoTracking();
//    Task<Controller?> TryGet(Guid id);
//    Task<Controller?> TryGetWithNoTracking(Guid id);
//    Task Upsert(Controller controller);
//    Task Delete(Guid id);
//}

//public interface IEquipmentRepository
//{
//    Task<List<Equipment>> GetAllWithNoTracking();
//    Task<Equipment?> TryGet(Guid id);
//    Task<Equipment?> TryGetWithNoTracking(Guid id);
//    Task Upsert(Equipment equipment);
//    Task Delete(Guid id);
//}

//public interface IEquipmentTemplateRepository
//{
//    Task<List<EquipmentTemplate>> GetAllWithNoTracking();
//    Task<EquipmentTemplate?> TryGet(Guid id);
//    Task<EquipmentTemplate?> TryGetWithNoTracking(Guid id);
//    Task Upsert(EquipmentTemplate equipmentTemplate);
//    Task Delete(Guid id);
//}

//public interface IMeasuringInstrumentRepository
//{
//    Task<List<MeasuringInstrument>> GetAllWithNoTracking();
//    Task<MeasuringInstrument?> TryGet(Guid id);
//    Task<MeasuringInstrument?> TryGetWithNoTracking(Guid id);
//    Task Upsert(MeasuringInstrument measuringInstrument);
//    Task Delete(Guid id);
//}

//public interface ITransducerRepository
//{
//    Task<List<Transducer>> GetAllWithNoTracking();
//    Task<Transducer?> TryGet(Guid id);
//    Task<Transducer?> TryGetWithNoTracking(Guid id);
//    Task Upsert(Transducer transducer);
//    Task Delete(Guid id);
//}

//public class MockControllerRepository : IControllerRepository
//{
//    private static readonly List<Controller> _controllers = new()
//    {
//        new Controller { Id = Guid.NewGuid(), Name = "PowerFocus 6000", SerialNumber = "PF6000-001", Identifier = "PF001", ConnectionMethod = ToolConnectionMethod.Ethernet, Type = ControllerType.PowerFocus6000 },
//        new Controller { Id = Guid.NewGuid(), Name = "PowerFocus 4000", SerialNumber = "PF4000-002", Identifier = "PF002", ConnectionMethod = ToolConnectionMethod.USB, Type = ControllerType.PowerFocus4000 }
//    };

//    public Task<List<Controller>> GetAllWithNoTracking() => Task.FromResult(_controllers.ToList());
//    public Task<Controller?> TryGet(Guid id) => Task.FromResult(_controllers.FirstOrDefault(c => c.Id == id));
//    public Task<Controller?> TryGetWithNoTracking(Guid id) => Task.FromResult(_controllers.FirstOrDefault(c => c.Id == id));

//    public Task Upsert(Controller controller)
//    {
//        var existing = _controllers.FirstOrDefault(c => c.Id == controller.Id);
//        if (existing != null)
//        {
//            _controllers.Remove(existing);
//        }
//        _controllers.Add(controller);
//        return Task.CompletedTask;
//    }

//    public Task Delete(Guid id)
//    {
//        var existing = _controllers.FirstOrDefault(c => c.Id == id);
//        if (existing != null)
//        {
//            _controllers.Remove(existing);
//        }
//        return Task.CompletedTask;
//    }
//}

//public class MockEquipmentTemplateRepository : IEquipmentTemplateRepository
//{
//    private static readonly List<EquipmentTemplate> _templates = new()
//    {
//        new EquipmentTemplate { Id = Guid.NewGuid(), Name = "Standard Torque Wrench", Type = EquipmentType.TorqueWrench, MeasurementUnit = UnitType.Nm, MinimumTorque = 10, MaximumTorque = 100 },
//        new EquipmentTemplate { Id = Guid.NewGuid(), Name = "Pneumatic Screwdriver", Type = EquipmentType.Screwdriver, MeasurementUnit = UnitType.Nm, MinimumTorque = 1, MaximumTorque = 25 }
//    };

//    public Task<List<EquipmentTemplate>> GetAllWithNoTracking() => Task.FromResult(_templates.ToList());
//    public Task<EquipmentTemplate?> TryGet(Guid id) => Task.FromResult(_templates.FirstOrDefault(t => t.Id == id));
//    public Task<EquipmentTemplate?> TryGetWithNoTracking(Guid id) => Task.FromResult(_templates.FirstOrDefault(t => t.Id == id));

//    public Task Upsert(EquipmentTemplate template)
//    {
//        var existing = _templates.FirstOrDefault(t => t.Id == template.Id);
//        if (existing != null) _templates.Remove(existing);
//        _templates.Add(template);
//        return Task.CompletedTask;
//    }

//    public Task Delete(Guid id)
//    {
//        var existing = _templates.FirstOrDefault(t => t.Id == id);
//        if (existing != null) _templates.Remove(existing);
//        return Task.CompletedTask;
//    }
//}

//public class MockEquipmentRepository : IEquipmentRepository
//{
//    private static readonly List<Equipment> _equipment = new();

//    public Task<List<Equipment>> GetAllWithNoTracking() => Task.FromResult(_equipment.ToList());
//    public Task<Equipment?> TryGet(Guid id) => Task.FromResult(_equipment.FirstOrDefault(e => e.Id == id));
//    public Task<Equipment?> TryGetWithNoTracking(Guid id) => Task.FromResult(_equipment.FirstOrDefault(e => e.Id == id));

//    public Task Upsert(Equipment equipment)
//    {
//        var existing = _equipment.FirstOrDefault(e => e.Id == equipment.Id);
//        if (existing != null) _equipment.Remove(existing);
//        _equipment.Add(equipment);
//        return Task.CompletedTask;
//    }

//    public Task Delete(Guid id)
//    {
//        var existing = _equipment.FirstOrDefault(e => e.Id == id);
//        if (existing != null) _equipment.Remove(existing);
//        return Task.CompletedTask;
//    }
//}

//public class MockMeasuringInstrumentRepository : IMeasuringInstrumentRepository
//{
//    private static readonly List<MeasuringInstrument> _instruments = new()
//    {
//        new MeasuringInstrument { Id = Guid.NewGuid(), Name = "STa6000 Standard", SerialNumber = "STA6000-001", ConnectionMethod = ToolConnectionMethod.Ethernet, Type = MeasuringInstrumentType.STa6000 }
//    };

//    public Task<List<MeasuringInstrument>> GetAllWithNoTracking() => Task.FromResult(_instruments.ToList());
//    public Task<MeasuringInstrument?> TryGet(Guid id) => Task.FromResult(_instruments.FirstOrDefault(i => i.Id == id));
//    public Task<MeasuringInstrument?> TryGetWithNoTracking(Guid id) => Task.FromResult(_instruments.FirstOrDefault(i => i.Id == id));

//    public Task Upsert(MeasuringInstrument instrument)
//    {
//        var existing = _instruments.FirstOrDefault(i => i.Id == instrument.Id);
//        if (existing != null) _instruments.Remove(existing);
//        _instruments.Add(instrument);
//        return Task.CompletedTask;
//    }

//    public Task Delete(Guid id)
//    {
//        var existing = _instruments.FirstOrDefault(i => i.Id == id);
//        if (existing != null) _instruments.Remove(existing);
//        return Task.CompletedTask;
//    }
//}

//public class MockTransducerRepository : ITransducerRepository
//{
//    private static readonly List<Transducer> _transducers = new()
//    {
//        new Transducer { Id = Guid.NewGuid(), Name = "Brake Transducer 100Nm", SerialNumber = "BT-100-001", MinimumCapacity = 0, MaximumCapacity = 100, Type = TransducersType.Brake, CapacityUnit = UnitType.Nm, MeasurementUnit = UnitType.Nm }
//    };

//    public Task<List<Transducer>> GetAllWithNoTracking() => Task.FromResult(_transducers.ToList());
//    public Task<Transducer?> TryGet(Guid id) => Task.FromResult(_transducers.FirstOrDefault(t => t.Id == id));
//    public Task<Transducer?> TryGetWithNoTracking(Guid id) => Task.FromResult(_transducers.FirstOrDefault(t => t.Id == id));

//    public Task Upsert(Transducer transducer)
//    {
//        var existing = _transducers.FirstOrDefault(t => t.Id == transducer.Id);
//        if (existing != null) _transducers.Remove(existing);
//        _transducers.Add(transducer);
//        return Task.CompletedTask;
//    }

//    public Task Delete(Guid id)
//    {
//        var existing = _transducers.FirstOrDefault(t => t.Id == id);
//        if (existing != null) _transducers.Remove(existing);
//        return Task.CompletedTask;
//    }
//}

