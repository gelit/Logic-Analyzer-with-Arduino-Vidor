// Copyright (C) 2022  Intel Corporation. All rights reserved.
// Your use of Intel Corporation's design tools, logic functions 
// and other software and tools, and any partner logic 
// functions, and any output files from any of the foregoing 
// (including device programming or simulation files), and any 
// associated documentation or information are expressly subject 
// to the terms and conditions of the Intel Program License 
// Subscription Agreement, the Intel Quartus Prime License Agreement,
// the Intel FPGA IP License Agreement, or other applicable license
// agreement, including, without limitation, that your use is for
// the sole purpose of programming logic devices manufactured by
// Intel and sold by Intel or its authorized distributors.  Please
// refer to the applicable agreement for further details, at
// https://fpgasoftware.intel.com/eula.

// Generated by Quartus Prime Version 22.1std.0 Build 915 10/25/2022 SC Lite Edition
// Created on Tue Feb  7 03:59:45 2023

// synthesis message_off 10175

`timescale 1ns/1ns

module SM2 (
    reset,iSM2clk,iStart,T[10:0],
    oByAv,oTshift,oClr2,oGo);

    input reset;
    input iSM2clk;
    input iStart;
    input [10:0] T;
    tri0 reset;
    tri0 iStart;
    tri0 [10:0] T;
    output oByAv;
    output oTshift;
    output oClr2;
    output oGo;
    reg oByAv;
    reg reg_oByAv;
    reg oTshift;
    reg reg_oTshift;
    reg oClr2;
    reg oGo;
    reg [5:0] fstate;
    reg [5:0] reg_fstate;
    parameter state1=0,state2=1,state3=2,state4=3,state5=4,state6=5;

    initial
    begin
        reg_oByAv <= 1'b0;
        reg_oTshift <= 1'b0;
    end

    always @(posedge iSM2clk)
    begin
        if (iSM2clk) begin
            fstate <= reg_fstate;
        end
    end

    always @(fstate or reset or iStart or T or reg_oByAv or reg_oTshift)
    begin
        if (reset) begin
            reg_fstate <= state1;
            reg_oByAv <= 1'b0;
            reg_oTshift <= 1'b0;
            oByAv <= 1'b0;
            oTshift <= 1'b0;
            oClr2 <= 1'b0;
            oGo <= 1'b0;
        end
        else begin
            reg_oByAv <= 1'b0;
            reg_oTshift <= 1'b0;
            oClr2 <= 1'b0;
            oGo <= 1'b0;
            oByAv <= 1'b0;
            oTshift <= 1'b0;
            case (fstate)
                state1: begin
                    if ((iStart == 1'b1))
                        reg_fstate <= state2;
                    // Inserting 'else' block to prevent latch inference
                    else
                        reg_fstate <= state1;

                    oGo <= 1'b0;

                    oClr2 <= 1'b1;

                    reg_oTshift <= 1'b0;

                    reg_oByAv <= 1'b0;
                end
                state2: begin
                    reg_fstate <= state3;

                    oGo <= 1'b0;

                    oClr2 <= 1'b1;

                    reg_oTshift <= 1'b0;

                    reg_oByAv <= 1'b0;
                end
                state3: begin
                    reg_fstate <= state4;

                    oGo <= 1'b0;

                    oClr2 <= 1'b0;

                    reg_oTshift <= 1'b1;

                    reg_oByAv <= 1'b0;
                end
                state4: begin
                    reg_fstate <= state5;

                    oGo <= 1'b0;

                    oClr2 <= 1'b0;

                    reg_oTshift <= 1'b1;

                    reg_oByAv <= 1'b1;
                end
                state5: begin
                    if ((T[10:0] < 11'b11101101100))
                        reg_fstate <= state4;
                    else if ((T[10:0] == 11'b11101101100))
                        reg_fstate <= state6;
                    // Inserting 'else' block to prevent latch inference
                    else
                        reg_fstate <= state5;

                    oGo <= 1'b0;

                    oClr2 <= 1'b0;

                    reg_oTshift <= 1'b1;

                    reg_oByAv <= 1'b0;
                end
                state6: begin
                    reg_fstate <= state1;

                    oGo <= 1'b1;

                    oClr2 <= 1'b0;

                    reg_oTshift <= 1'b0;

                    reg_oByAv <= 1'b0;
                end
                default: begin
                    reg_oByAv <= 1'bx;
                    reg_oTshift <= 1'bx;
                    oClr2 <= 1'bx;
                    oGo <= 1'bx;
                    $display ("Reach undefined state");
                end
            endcase
            oByAv <= reg_oByAv;
            oTshift <= reg_oTshift;
        end
    end
endmodule // SM2
