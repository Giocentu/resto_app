using Avalonia.Controls;
using Avalonia.Interactivity;
using RestoApp.Entities;
using System;
using System.Collections.Generic;

namespace RestoApp.Desktop.Views;

public partial class AutorizacionRestoreWindow : Window
{
    private readonly List<UsuarioAutorizanteItem> _usuarios = new();

    public AutorizacionRestoreWindow()
    {
        InitializeComponent();
    }

    public AutorizacionRestoreWindow(List<UsuarioAutorizanteItem> usuarios)
    {
        InitializeComponent();
        _usuarios = usuarios ?? new List<UsuarioAutorizanteItem>();

        CmbUsuario1.ItemsSource = _usuarios;
        CmbUsuario2.ItemsSource = _usuarios;

        if (_usuarios.Count > 0)
        {
            CmbUsuario1.SelectedIndex = 0;
            if (_usuarios.Count > 1)
            {
                CmbUsuario2.SelectedIndex = 1;
            }
        }
    }

    private void BtnConfirmar_Click(object? sender, RoutedEventArgs e)
    {
        TxtError.IsVisible = false;

        var u1 = CmbUsuario1.SelectedItem as UsuarioAutorizanteItem;
        var u2 = CmbUsuario2.SelectedItem as UsuarioAutorizanteItem;

        if (u1 is null)
        {
            MostrarError("Por favor seleccione el primer usuario autorizante.");
            return;
        }

        if (u2 is null)
        {
            MostrarError("Por favor seleccione el segundo usuario autorizante.");
            return;
        }

        if (string.Equals(u1.Id, u2.Id, StringComparison.OrdinalIgnoreCase))
        {
            MostrarError("Los dos autorizantes deben ser usuarios diferentes.");
            return;
        }

        var pass1 = TxtPassword1.Text ?? string.Empty;
        var pass2 = TxtPassword2.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(pass1))
        {
            MostrarError($"Ingrese la contraseña para {u1.NombreCompleto}.");
            return;
        }

        if (string.IsNullOrWhiteSpace(pass2))
        {
            MostrarError($"Ingrese la contraseña para {u2.NombreCompleto}.");
            return;
        }

        if (!string.Equals(pass1, u1.PasswordEsperada, StringComparison.Ordinal))
        {
            MostrarError($"La contraseña para {u1.NombreCompleto} es incorrecta.");
            return;
        }

        if (!string.Equals(pass2, u2.PasswordEsperada, StringComparison.Ordinal))
        {
            MostrarError($"La contraseña para {u2.NombreCompleto} es incorrecta.");
            return;
        }

        // Ambas contraseñas son válidas y provienen de usuarios distintos
        Close(true);
    }

    private void BtnCancelar_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    private void MostrarError(string mensaje)
    {
        TxtError.Text = mensaje;
        TxtError.IsVisible = true;
    }
}

