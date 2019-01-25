﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MySql.Data.MySqlClient;

public class Database : MonoBehaviour
{


    public static MySqlConnection c = new MySqlConnection("Server=localhost;Database=babystar;User id=root;Password=;Connection Timeout=10;");
    public static MySqlCommand cmd = new MySqlCommand("", c);

    public static int GetCriteriaID(string crit)
    {
        int idCrit = 1;
        cmd.CommandText = "SELECT id FROM criteria WHERE name = '" + crit + "'";
        c.Open();
        int.TryParse(cmd.ExecuteScalar().ToString(), out idCrit);
        c.Close();
        return idCrit;
    }


    public static void SetScore(string crit, int score, int scoreMax)
    {
        int idCrit = GetCriteriaID(crit);
        cmd.CommandText = "INSERT INTO main (idCrit, score, maxScore, idUser, date) VALUES( " + idCrit + ", " + score + ", " + scoreMax + ", " + 1 + ", #" + DateTime.Now.ToShortDateString() + "#)";
        c.Open();
        cmd.ExecuteNonQuery();
        c.Close();
    }
}