// // This file is auto-generated from InputSystem_Actions.inputactions
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine.InputSystem;
// using UnityEngine.InputSystem.Utilities;

// namespace ALUNGAMES
// {
//     public class InputSystem_Actions : IInputActionCollection, IDisposable
//     {
//         private InputActionAsset asset;
        
//         public InputSystem_Actions()
//         {
//             asset = InputActionAsset.FromJson(@"{
//                 ""name"": ""InputSystem_Actions"",
//                 ""maps"": [
//                     {
//                         ""name"": ""Player"",
//                         ""id"": ""df70fa95-8a34-4494-b137-73ab6b9c7d37"",
//                         ""actions"": [
//                             {
//                                 ""name"": ""Move"",
//                                 ""type"": ""Value"",
//                                 ""id"": ""351f2ccd-1f9f-44bf-9bec-d62ac5c5f408"",
//                                 ""expectedControlType"": ""Vector2"",
//                                 ""processors"": """",
//                                 ""interactions"": """",
//                                 ""initialStateCheck"": true
//                             },
//                             {
//                                 ""name"": ""Interact"",
//                                 ""type"": ""Button"",
//                                 ""id"": ""852140f2-7766-474d-8707-702459ba45f3"",
//                                 ""expectedControlType"": ""Button"",
//                                 ""processors"": """",
//                                 ""interactions"": ""Hold"",
//                                 ""initialStateCheck"": false
//                             }
//                         ]
//                     }
//                 ]
//             }");
            
//             // Player map
//             m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
//             m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
//             m_Player_Interact = m_Player.FindAction("Interact", throwIfNotFound: true);
//         }

//         public void Dispose()
//         {
//             UnityEngine.Object.Destroy(asset);
//         }

//         public InputBinding? bindingMask
//         {
//             get => asset.bindingMask;
//             set => asset.bindingMask = value;
//         }

//         public ReadOnlyArray<InputDevice>? devices
//         {
//             get => asset.devices;
//             set => asset.devices = value;
//         }

//         public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

//         public bool Contains(InputAction action)
//         {
//             return asset.Contains(action);
//         }

//         public IEnumerator<InputAction> GetEnumerator()
//         {
//             return asset.GetEnumerator();
//         }

//         IEnumerator IEnumerable.GetEnumerator()
//         {
//             return GetEnumerator();
//         }

//         public void Enable()
//         {
//             asset.Enable();
//         }

//         public void Disable()
//         {
//             asset.Disable();
//         }

//         // Player
//         private readonly InputActionMap m_Player;
//         private IPlayerActions m_PlayerActionsCallbackInterface;
//         private readonly InputAction m_Player_Move;
//         private readonly InputAction m_Player_Interact;
        
//         public struct PlayerActions
//         {
//             private InputSystem_Actions m_Wrapper;
            
//             public PlayerActions(InputSystem_Actions wrapper) { m_Wrapper = wrapper; }
            
//             public InputAction @Move => m_Wrapper.m_Player_Move;
//             public InputAction @Interact => m_Wrapper.m_Player_Interact;
            
//             public InputActionMap Get() { return m_Wrapper.m_Player; }
            
//             public void Enable() { Get().Enable(); }
//             public void Disable() { Get().Disable(); }
//             public bool enabled => Get().enabled;
            
//             public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
            
//             public void SetCallbacks(IPlayerActions instance)
//             {
//                 if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
//                 {
//                     @Move.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
//                     @Move.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
//                     @Move.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
//                     @Interact.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
//                     @Interact.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
//                     @Interact.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnInteract;
//                 }
                
//                 m_Wrapper.m_PlayerActionsCallbackInterface = instance;
//                 if (instance != null)
//                 {
//                     @Move.started += instance.OnMove;
//                     @Move.performed += instance.OnMove;
//                     @Move.canceled += instance.OnMove;
//                     @Interact.started += instance.OnInteract;
//                     @Interact.performed += instance.OnInteract;
//                     @Interact.canceled += instance.OnInteract;
//                 }
//             }
//         }
        
//         public PlayerActions @Player => new PlayerActions(this);
        
//         public interface IPlayerActions
//         {
//             void OnMove(InputAction.CallbackContext context);
//             void OnInteract(InputAction.CallbackContext context);
//         }
//     }
// } 