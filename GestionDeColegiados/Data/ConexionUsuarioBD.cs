﻿using System;
using System.Data;

using MySql.Data.MySqlClient;

using Sistema;

namespace Model {
  public class ConexionUsuarioBD {
    //variables necesarias para leer y realizar consultas con exitos de la base de datos
    private MySqlDataReader _reader;
    private MySqlConnection _conexion;
    private MySqlTransaction _transaccion = null;

    //metodo que nos devuelve el administrador creado, si es que el mismo existe
    public Administrador ExisteUsuario(string usuario, string pass) {
      //obtenemos la conexion
      _conexion = ConexionBD.GetConexion();
      // abrimos la conexion
      _conexion.Open();

      //llamamos al procedimiento almacenado creado

      MySqlCommand comando = new MySqlCommand("login", _conexion);
      //informamos que el comando a enviar es un procedimiento almacenado
      comando.CommandType = CommandType.StoredProcedure;
      /*
       * Aqui enviamos los parametros necesarios para el procedimiento almacenado
       *  el primer parametro es el nombre del campo en el procedimiento almacenadi
       *  el segundo parametro es la variable que contiene el valor para el campo
       */
      comando.Parameters.AddWithValue("@_username", usuario);
      comando.Parameters.AddWithValue("@_pass", pass);
      //ejecutamos dicho procedimiento
      _reader = comando.ExecuteReader();
      // declaramos un administrador como null 
      Administrador administrador = null;
      //si la consulta es real entonces procedemos a instanciar dicho administrador
      if(_reader.Read()) {
        administrador = new Administrador();
        administrador.Id = int.Parse(_reader["Id"].ToString());
        administrador.Nombre = _reader["UserName"].ToString();
        administrador.Password = _reader["UserPassword"].ToString();
        administrador.Rol = _reader["rol"].ToString();
        administrador.PrimerAcceso = _reader["primerAcceso"].ToString();
      }
      //cerramos la conexion
      _conexion.Close();
      //finalmente lo retornamos
      return administrador;
    }

    public string CambiarPassword(string newPass, int idUser) {
      string respuesta = "";
      _conexion = ConexionBD.GetConexion();
      _conexion.Open();
      _transaccion = _conexion.BeginTransaction();
      try {
        MySqlCommand cmd = new MySqlCommand("cambiarPass", _conexion, _transaccion);

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@_newPass", newPass);
        cmd.Parameters.AddWithValue("@_idUser", idUser);
        cmd.Parameters.AddWithValue("@_primerAcceso", DateTime.Now.ToString("yyyy-MM-dd"));

        cmd.ExecuteNonQuery();

        _transaccion.Commit();

        respuesta = "EXITO: Cambio de contraseña exitoso";

      } catch(MySqlException ex) {
        _transaccion.Rollback();
        respuesta = "ERROR: Cambio de contraseña no exitoso";
        throw new Exception(ex.ToString());

      } finally {
        _conexion.Close();
      }
      return respuesta;
    }
  }
}