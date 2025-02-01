using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private Animator unitAnimator = null;
    [SerializeField] private NavMeshAgent agent = null;

    private Camera mainCamera;

    [SyncVar]
    bool _isRunning;

    #region Server

    [Command]
    private void CmdMove(Vector3 position)
    {
        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }

        agent.SetDestination(hit.position);
    }

    [Command]
    public void CmdSetRun(bool running)
    {
        _isRunning = running;
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        mainCamera = Camera.main;
    }

    [ClientCallback]
    private void Update()
    {
        CmdSetRun(agent.velocity.magnitude > 0f);

        unitAnimator.SetBool("isRunning", _isRunning);

        //if (!hasAuthority) { return; }

        if (!Mouse.current.rightButton.wasPressedThisFrame) { return; }

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) { return; }

        CmdMove(hit.point);

        agent.SetDestination(hit.point);

    }

    #endregion
}
