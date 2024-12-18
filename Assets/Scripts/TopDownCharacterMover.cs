﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputHandler))]
public class TopDownCharacterMover : MonoBehaviour
{
    private InputHandler _input;

    [SerializeField]
    private bool RotateTowardMouse;

    [SerializeField]
    private float MovementSpeed;
    [SerializeField]
    private float RotationSpeed;

    [SerializeField]
    private Camera Camera;

    [Header("Shooting")]
    [SerializeField]
    private GameObject ProjectilePrefab;

    [SerializeField]
    private Transform ProjectileSpawnPoint;

    [SerializeField]
    private float ProjectileSpeed = 10f;

    [SerializeField]
    private float ShootingCooldown = 0.5f; // Time between shots
    private float _lastShotTime;

    private void Awake()
    {
        _input = GetComponent<InputHandler>();
    }

    void Update()
    {
        var targetVector = new Vector3(_input.InputVector.x, 0, _input.InputVector.y);
        var movementVector = MoveTowardTarget(targetVector);

        if (!RotateTowardMouse)
        {
            RotateTowardMovementVector(movementVector);
        }
        if (RotateTowardMouse)
        {
            RotateFromMouseVector();
        }

        if (_input.IsShooting && Time.time >= _lastShotTime + ShootingCooldown)
        {
            ShootProjectile();
            _lastShotTime = Time.time;
        }
    }

    private void RotateFromMouseVector()
    {
        Ray ray = Camera.ScreenPointToRay(_input.MousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f))
        {
            var target = hitInfo.point;
            target.y = transform.position.y;
            transform.LookAt(target);
        }
    }

    private Vector3 MoveTowardTarget(Vector3 targetVector)
    {
        var speed = MovementSpeed * Time.deltaTime;

        targetVector = Quaternion.Euler(0, Camera.gameObject.transform.rotation.eulerAngles.y, 0) * targetVector;
        var targetPosition = transform.position + targetVector * speed;
        transform.position = targetPosition;

        return targetVector;
    }

    private void RotateTowardMovementVector(Vector3 movementDirection)
    {
        if (movementDirection.magnitude == 0) { return; }
        var rotation = Quaternion.LookRotation(movementDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, RotationSpeed);
    }

    private void ShootProjectile()
    {
        if (ProjectilePrefab == null || ProjectileSpawnPoint == null)
        {
            Debug.LogWarning("ProjectilePrefab or ProjectileSpawnPoint is not set!");
            return;
        }

        // Instantiate the projectile
        var projectile = Instantiate(ProjectilePrefab, ProjectileSpawnPoint.position, ProjectileSpawnPoint.rotation);

        // Set its velocity in the forward direction of the spawn point
        var rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = ProjectileSpawnPoint.forward * ProjectileSpeed;
        }

        // Align the projectile's rotation to its forward movement direction
        projectile.transform.rotation = Quaternion.LookRotation(ProjectileSpawnPoint.forward);
    }
}
